using Abstracciones.Modelos;
namespace Abstracciones.Flujo
{
    public interface ICredencialesFlujo
    {
        Task<ResultadoInvitacion> Registrar(RegistroAdministrativoRequest solicitud,int empresa,int actor,CancellationToken ct);
        Task<ResultadoInvitacion> Restablecer(int usuario,int empresa,int actor,CancellationToken ct);
        Task Recuperar(string correo,CancellationToken ct);
        Task<bool> Consumir(string token,string contrasena,CancellationToken ct);
    }
}