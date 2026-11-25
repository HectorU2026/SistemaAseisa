using Abstracciones.Modelos;

namespace Abstracciones.Flujo
{
    public interface IAuditoriaSeguridad
    {
        Task RegistrarAsync(EventoAuditoriaSeguridad evento, int? idUsuario = null, string? correo = null);
    }
}
