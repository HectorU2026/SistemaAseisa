using ASEISA.Plataforma;
using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Seguridad.Criptografia;
namespace Flujo;
public sealed class PlataformaCredencialesFlujo:IPlataformaCredencialesFlujo
{
    private readonly IPlataformaCredencialesDA da;private readonly ICredencialesFlujo credenciales;private readonly IProtectorEnlace protector;private readonly IEntregaEnlace entrega;
    public PlataformaCredencialesFlujo(IPlataformaCredencialesDA da,ICredencialesFlujo credenciales,IProtectorEnlace protector,IEntregaEnlace entrega){this.da=da;this.credenciales=credenciales;this.protector=protector;this.entrega=entrega;}
    public async Task<ResultadoInvitacion> Registrar(int actor,int empresa,PersonaPlataforma p,CancellationToken ct)
    {
        Validator.ValidateObject(p,new ValidationContext(p),true);
        var catalogo=await da.Validar(actor,empresa,null,ct);
        return await credenciales.Registrar(new RegistroAdministrativoRequest {
            Nombre=p.Nombre,PrimerApellido=p.PrimerApellido,SegundoApellido=p.SegundoApellido,Correo=p.Correo,TipoCliente=p.TipoCliente,
            Identificacion=p.Identificacion,NombreRazonSocial=p.NombreRazonSocial,Telefono=p.Telefono,Direccion=p.Direccion,
            IdRol=catalogo.Rol,IdsRoles=new(){catalogo.Rol},IdEstado=catalogo.Estado},empresa,actor,ct);
    }
    public async Task<ResultadoInvitacion> Restablecer(int actor,int empresa,int usuario,CancellationToken ct)
    {
        await da.Validar(actor,empresa,usuario,ct);
        return await credenciales.Restablecer(usuario,empresa,actor,ct);
    }
    public Task<int> Inicializar(PersonaPlataforma p,string destino,CancellationToken ct)
    {
        Validator.ValidateObject(p,new ValidationContext(p),true);
        protector.ValidarConfiguracion();entrega.ValidarConfiguracion();
        var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        // Credencial inicial aleatoria desconocida. El primer acceso requiere establecerla mediante el enlace.
        var hash=ContrasenasSeguras.Crear(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
        var usuario=ASEISA.Usuarios.ReglasRegistroUsuario.Generar(p.Nombre,p.PrimerApellido,p.Identificacion);
        if(string.IsNullOrWhiteSpace(usuario)||string.IsNullOrWhiteSpace(destino))throw new ArgumentException("Identidad o destino incompletos.");
        return da.Inicializar(p,usuario,hash,SHA256.HashData(Encoding.UTF8.GetBytes(token)),protector.Cifrar(token),destino,ct);
    }
}