using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Microsoft.Extensions.Logging;

namespace Flujo
{
    public class AuditoriaSeguridad : IAuditoriaSeguridad
    {
        private readonly IAuditoriaSeguridadDA _auditoriaDA;
        private readonly ILogger<AuditoriaSeguridad> _logger;

        public AuditoriaSeguridad(IAuditoriaSeguridadDA auditoriaDA, ILogger<AuditoriaSeguridad> logger)
        {
            _auditoriaDA = auditoriaDA;
            _logger = logger;
        }

        public async Task RegistrarAsync(EventoAuditoriaSeguridad evento, int? idUsuario = null, string? correo = null)
        {
            try
            {
                await _auditoriaDA.RegistrarAsync(evento, idUsuario, correo);
            }
            catch (Exception)
            {
                // No registrar la excepcion: puede contener SQL, parametros o credenciales.
                // La operacion principal ya termino; no comunicar un falso fracaso al cliente.
                _logger.LogError(new EventId(1001, "AuditoriaSeguridadFallida"),
                    "No se pudo persistir el evento de seguridad {Evento}.", evento);
            }
        }
    }
}
