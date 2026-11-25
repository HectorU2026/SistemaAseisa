using API.Seguridad;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Abstracciones.DA;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Flujo;
using System.Security.Cryptography;
using System.Text;
using Seguridad.Criptografia;
internal static class PruebasCredenciales
{
    public static async Task<int> Ejecutar()
    {
        var casos=0;
        void Verificar(bool valor,string nombre){if(!valor)throw new Exception(nombre);Console.WriteLine("OK "+nombre);casos++;}
        var da=new CredencialesFake();var entrega=new EntregaFake();var protector=new ProtectorFake();var flujo=new CredencialesFlujo(da,entrega,protector);
        var creado=await flujo.Registrar(new RegistroAdministrativoRequest {Nombre="Usuario",PrimerApellido="Prueba",Identificacion="123456789"},8,7,default);
        Verificar(creado.IdUsuario==12 && creado.EnvioProgramado && !creado.EnlaceEnviado,"Alta confirma programacion, no entrega SMTP");
        var token=protector.Descifrar(da.Cifrado,da.Resumen!);
        Verificar(da.HashInicial.StartsWith("PBKDF2-SHA256$") && token.Length==64,"Credencial inicial aleatoria");
        Verificar(da.Resumen!.SequenceEqual(SHA256.HashData(Encoding.UTF8.GetBytes(token))),"Hash de solicitud coincide con sobre cifrado");
        Verificar(!da.Cifrado.Contains(token) && !da.HashInicial.Contains(token),"No persiste token plano ni como contrasena");
        var restablecido=await flujo.Restablecer(12,8,7,default);
        Verificar(restablecido.EnvioProgramado && entrega.Envios==0,"Restablecimiento no depende de SMTP en la peticion");
        var llamadas=da.Consumos;
        Verificar(!await flujo.Consumir("JWT-no-valido","ContrasenaSinteticaLarga",default) && da.Consumos==llamadas,"No acepta JWT de recuperacion");
        Verificar(!await flujo.Consumir(new string('A',64),"corta",default) && da.Consumos==llamadas,"Longitud validada antes de SQL");
        Verificar(await flujo.Consumir(new string('A',64),"ContrasenaSinteticaLarga",default)
            && ContrasenasSeguras.Verificar("ContrasenaSinteticaLarga",da.HashConsumido,out _),"Cambio genera hash seguro");
        da.ResultadoConsumo=false;
        Verificar(!await flujo.Consumir(new string('A',64),"ContrasenaSinteticaLarga",default),"Rechazo de consumo no disponible");
        await flujo.Recuperar("ficticio@example.invalid",default);
        Verificar(entrega.Envios==0 && da.Solicitudes==2,"Recuperacion persiste sin intentar envio directo");
        da.Destino=null;await flujo.Recuperar("inexistente@example.invalid",default);
        Verificar(entrega.Envios==0,"Cuenta desconocida no envia correo");
        protector.Configurado=false;var registros=da.Registros;try{await flujo.Registrar(new(),8,7,default);throw new Exception("No valido configuracion");}catch(InvalidOperationException){}
        Verificar(da.Registros==registros,"Clave ausente impide registrar sin bandeja cifrada");
        var clave=RandomNumberGenerator.GetBytes(32);var secreto=new string('A',64);var hash=SHA256.HashData(Encoding.UTF8.GetBytes(secreto));
        var cifrado=CifradoEnlaces.Cifrar(secreto,clave);
        Verificar(CifradoEnlaces.Descifrar(cifrado,clave,hash)==secreto,"Sobre AES-GCM recuperable tras reinicio");
        Verificar(CifradoEnlaces.Cifrar(secreto,clave)!=cifrado,"Nonce aleatorio por sobre");
        void DebeFallar(Action accion,string nombre)
        {try{accion();throw new Exception(nombre);}catch(CryptographicException){Console.WriteLine("OK "+nombre);casos++;}}
        DebeFallar(()=>CifradoEnlaces.Descifrar(cifrado,RandomNumberGenerator.GetBytes(32),hash),"Clave incorrecta rechazada");
        DebeFallar(()=>CifradoEnlaces.Descifrar(cifrado,clave,new byte[32]),"Sobre vinculado a su solicitud");
        var bytes=Convert.FromBase64String(cifrado[3..]);bytes[^1]^=1;
        DebeFallar(()=>CifradoEnlaces.Descifrar("v1."+Convert.ToBase64String(bytes),clave,hash),"Cifrado manipulado rechazado");
                var correos=new CorreosFake();var entregaWorker=new EntregaFake { Exito=false };var protectorWorker=new ProtectorFake();
        var tokenWorker=new string('B',64);var reserva=Guid.NewGuid();
        correos.Proximo=new(){Procesable=true,IdCorreo=1,Reserva=reserva,IdUsuario=12,Nombre="Prueba",Correo="ficticio@example.invalid",
            TokenCifrado=protectorWorker.Cifrar(tokenWorker),HashToken=SHA256.HashData(Encoding.UTF8.GetBytes(tokenWorker))};
        var servicios=new ServiceCollection();servicios.AddSingleton<ICorreoEnlaceDA>(correos);
        servicios.AddSingleton<IProtectorEnlace>(protectorWorker);servicios.AddSingleton<IEntregaEnlace>(entregaWorker);
        using var proveedor=servicios.BuildServiceProvider();
        var worker=new DespachadorEnlaces(proveedor.GetRequiredService<IServiceScopeFactory>(),NullLogger<DespachadorEnlaces>.Instance);
        Verificar(await worker.Procesar(default) && correos.UltimaReserva==reserva && !correos.Enviado,"Fallo SMTP confirma reintento con reserva correcta");
        Verificar(!await worker.Procesar(default),"Bandeja vacia no intenta correo");
        entregaWorker.Exito=true;
        correos.Proximo=new(){Procesable=true,IdCorreo=2,Reserva=Guid.NewGuid(),TokenCifrado=protectorWorker.Cifrar(tokenWorker),HashToken=SHA256.HashData(Encoding.UTF8.GetBytes(tokenWorker))};
        var reiniciado=new DespachadorEnlaces(proveedor.GetRequiredService<IServiceScopeFactory>(),NullLogger<DespachadorEnlaces>.Instance);
        Verificar(await reiniciado.Procesar(default) && correos.Enviado,"Nuevo despachador procesa trabajo persistido");
        var envios=entregaWorker.Envios;
        correos.Proximo=new(){Procesable=true,IdCorreo=3,Reserva=Guid.NewGuid(),TokenCifrado="v1.invalido",HashToken=new byte[32]};
        Verificar(await worker.Procesar(default) && !correos.Enviado && entregaWorker.Envios==envios,"No envia sobres invalidos");
        correos.Proximo=new(){IdCorreo=4,Procesable=false};envios=entregaWorker.Envios;
        Verificar(await worker.Procesar(default) && entregaWorker.Envios==envios,"Trabajo anulado no retrasa los siguientes envios");
        CryptographicOperations.ZeroMemory(clave);return casos;
    }
    private sealed class CredencialesFake:ICredencialesDA
    {
        public DestinatarioContrasena? Destino=new(){IdUsuario=12,Nombre="Prueba",Correo="ficticio@example.invalid"};
        public string HashInicial="",HashConsumido="",Cifrado="";public byte[]? Resumen;public int Consumos,Registros,Solicitudes;public bool ResultadoConsumo=true;
        public Task<(int IdUsuario,DestinatarioContrasena? Destinatario)> Registrar(RegistroAdministrativoRequest s,int e,int a,string h,byte[] t,string cifrado,CancellationToken ct)
        {Registros++;HashInicial=h;Resumen=t;Cifrado=cifrado;return Task.FromResult((12,Destino));}
        public Task<DestinatarioContrasena?> Solicitar(byte[] t,string cifrado,string p,string? c,int? u,int? e,int? a,CancellationToken ct)
        {Solicitudes++;Resumen=t;Cifrado=cifrado;return Task.FromResult(Destino);}
        public Task<bool> Consumir(byte[] t,string h,CancellationToken ct){Consumos++;HashConsumido=h;return Task.FromResult(ResultadoConsumo);}
    }
    private sealed class EntregaFake:IEntregaEnlace
    {
        public int Envios;public bool Exito=true;
        public void ValidarConfiguracion(){}
        public Task<bool> Enviar(DestinatarioContrasena d,string token){Envios++;return Task.FromResult(Exito);}
    }
    private sealed class ProtectorFake:IProtectorEnlace
    {
        private readonly byte[] _clave=RandomNumberGenerator.GetBytes(32);public bool Configurado=true;
        public void ValidarConfiguracion(){if(!Configurado)throw new InvalidOperationException();}
        public string Cifrar(string token)=>CifradoEnlaces.Cifrar(token,_clave);
        public string Descifrar(string cifrado,byte[] hash)=>CifradoEnlaces.Descifrar(cifrado,_clave,hash);
    }
    private sealed class CorreosFake:ICorreoEnlaceDA
    {
        public CorreoEnlacePendiente? Proximo;public bool Enviado;public Guid UltimaReserva;
        public Task<CorreoEnlacePendiente?> Tomar(CancellationToken ct){var r=Proximo;Proximo=null;return Task.FromResult(r);}
        public Task Finalizar(long id,Guid reserva,bool enviado,CancellationToken ct){UltimaReserva=reserva;Enviado=enviado;return Task.CompletedTask;}
    }}