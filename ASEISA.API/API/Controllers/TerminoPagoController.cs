using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class TerminoPagoController : ControllerBase
    {
        private readonly ITerminoPagoFlujo _terminoPagoFlujo;
        private readonly IUsuarioFlujo _usuarioFlujo;
        private readonly ILogger<TerminoPagoController> _logger;

        public TerminoPagoController(
            ITerminoPagoFlujo terminoPagoFlujo,
            IUsuarioFlujo usuarioFlujo,
            ILogger<TerminoPagoController> logger)
        {
            _terminoPagoFlujo = terminoPagoFlujo;
            _usuarioFlujo = usuarioFlujo;
            _logger = logger;
        }

        #region "Operaciones"

        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            var resultado = await _terminoPagoFlujo.Obtener(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario);

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(
            [FromBody] TerminoPagoRequest termino)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                var resultado = await _terminoPagoFlujo.Agregar(
                    idEmpresa: idEmpresa,
                    idUsuario: idUsuario,
                    termino: termino);

                return Created(
                    $"/api/TerminoPago/{resultado.IdTerminoPago}",
                    resultado);
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                _logger.LogWarning(
                    ex,
                    "Nombre duplicado al registrar término de pago. Empresa: {IdEmpresa}",
                    idEmpresa);

                return Conflict(new
                {
                    mensaje = "Ya existe un término de pago con ese nombre."
                });
            }
        }

        [HttpPut("{idTerminoPago:int}")]
        public async Task<IActionResult> Editar(
            [FromRoute] int idTerminoPago,
            [FromBody] TerminoPagoRequest termino)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                await _terminoPagoFlujo.Editar(
                    idEmpresa: idEmpresa,
                    idUsuario: idUsuario,
                    idTerminoPago: idTerminoPago,
                    termino: termino);

                return NoContent();
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un término de pago con ese nombre."
                });
            }
        }

        [HttpPut("{idTerminoPago:int}/activar")]
        public async Task<IActionResult> Activar(
            [FromRoute] int idTerminoPago)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            await _terminoPagoFlujo.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idTerminoPago: idTerminoPago,
                activo: true);

            return NoContent();
        }

        [HttpPut("{idTerminoPago:int}/desactivar")]
        public async Task<IActionResult> Desactivar(
            [FromRoute] int idTerminoPago)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            await _terminoPagoFlujo.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idTerminoPago: idTerminoPago,
                activo: false);

            return NoContent();
        }

        #endregion

        #region "Helpers"

        private static bool EsNombreDuplicado(SqlException ex)
        {
            return ex.Number == 2601 ||
                   ex.Number == 2627 ||
                   ex.Message.Contains(
                       "duplicado",
                       StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains(
                       "ya existe",
                       StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(int IdEmpresa, int IdUsuario)?>
            ObtenerContextoUsuario()
        {
            var valor = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) ||
                idUsuario <= 0)
            {
                return null;
            }

            var idEmpresa =
                await _usuarioFlujo.ObtenerEmpresaUsuarioActivo(
                    idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return null;

            return (idEmpresa.Value, idUsuario);
        }

        #endregion
    }
}
