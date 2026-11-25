using Abstracciones.Modelos;
namespace Abstracciones.Interfaces.Servicio
{
    public interface IAdministracionCredenciales
    {
        Task<ResultadoInvitacion> Registrar(UsuarioRequest solicitud,CancellationToken ct);
        Task<ResultadoInvitacion> Restablecer(int idUsuario,CancellationToken ct);
    }
}