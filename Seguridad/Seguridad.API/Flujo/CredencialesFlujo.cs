using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Seguridad.Criptografia;
using System.Security.Cryptography;
using System.Text;
namespace Flujo
{
    public sealed class CredencialesFlujo:ICredencialesFlujo
    {
        private readonly ICredencialesDA _da;
        private readonly IEntregaEnlace _entrega;
        private readonly IProtectorEnlace _protector;
        public CredencialesFlujo(ICredencialesDA da,IEntregaEnlace entrega,IProtectorEnlace protector)
        {_da=da;_entrega=entrega;_protector=protector;}
        private static string Token()=>Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        private static byte[] Resumen(string token)=>SHA256.HashData(Encoding.UTF8.GetBytes(token));
        private void Validar(){_entrega.ValidarConfiguracion();_protector.ValidarConfiguracion();}
        public async Task<ResultadoInvitacion> Registrar(RegistroAdministrativoRequest solicitud,int empresa,int actor,CancellationToken ct)
        {
            Validar();var token=Token();
            solicitud.NombreUsuario=ASEISA.Usuarios.ReglasRegistroUsuario.Generar(solicitud.Nombre,solicitud.PrimerApellido,solicitud.Identificacion);
            if(string.IsNullOrWhiteSpace(solicitud.NombreUsuario))throw new ArgumentException("Datos de identidad incompletos.");
            var resultado=await _da.Registrar(solicitud,empresa,actor,ContrasenasSeguras.Crear(Token()),Resumen(token),_protector.Cifrar(token),ct);
            return new(){IdUsuario=resultado.IdUsuario,EnvioProgramado=resultado.Destinatario!=null};
        }
        public async Task<ResultadoInvitacion> Restablecer(int usuario,int empresa,int actor,CancellationToken ct)
        {
            Validar();var token=Token();
            var destino=await _da.Solicitar(Resumen(token),_protector.Cifrar(token),"Administrativo",null,usuario,empresa,actor,ct);
            return new(){IdUsuario=usuario,EnvioProgramado=destino!=null};
        }
        public async Task Recuperar(string correo,CancellationToken ct)
        {
            Validar();var token=Token();
            await _da.Solicitar(Resumen(token),_protector.Cifrar(token),"Recuperacion",correo,null,null,null,ct);
        }
        public Task<bool> Consumir(string token,string contrasena,CancellationToken ct)
        {
            if(token.Length!=64 || !token.All(Uri.IsHexDigit) || string.IsNullOrWhiteSpace(contrasena) || contrasena.Length<12 || contrasena.Length>128)
                return Task.FromResult(false);
            return _da.Consumir(Resumen(token),ContrasenasSeguras.Crear(contrasena),ct);
        }
    }
}