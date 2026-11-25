using Abstracciones.Modelos;
namespace Abstracciones.Interfaces.Servicio
{
    public interface IAdministracionCredenciales
    {
        Task<ResultadoInvitacion> RegistrarPlataforma(int empresa,ASEISA.Plataforma.PersonaPlataforma datos,CancellationToken ct);
        Task<ResultadoInvitacion> RestablecerPlataforma(int empresa,int usuario,CancellationToken ct);
        Task<ResultadoInvitacion> Registrar(UsuarioRequest solicitud,CancellationToken ct);
        Task<ResultadoInvitacion> Restablecer(int idUsuario,CancellationToken ct);
    }
}