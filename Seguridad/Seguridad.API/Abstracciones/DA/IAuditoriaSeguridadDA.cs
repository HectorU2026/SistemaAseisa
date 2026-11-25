using Abstracciones.Modelos;

namespace Abstracciones.DA
{
    public interface IAuditoriaSeguridadDA
    {
        Task RegistrarAsync(EventoAuditoriaSeguridad evento, int? idUsuario, string? correo);
    }
}
