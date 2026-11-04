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
    public class MetodoPagoController : ControllerBase
    {
        private readonly IMetodoPagoFlujo _metodoPagoFlujo;
        private readonly IUsuarioFlujo _usuarioFlujo;
        private readonly ILogger<MetodoPagoController> _logger;

        public MetodoPagoController(
            IMetodoPagoFlujo metodoPagoFlujo,
            IUsuarioFlujo usuarioFlujo,
            ILogger<MetodoPagoController> logger)
        {
            _metodoPagoFlujo = metodoPagoFlujo;
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

            var resultado = await _metodoPagoFlujo.Obtener(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario);

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(
            [FromBody] MetodoPagoRequest metodo)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                var resultado = await _metodoPagoFlujo.Agregar(
                    idEmpresa: idEmpresa,
                    idUsuario: idUsuario,
                    metodo: metodo);

                return Created(
                    $"/api/MetodoPago/{resultado.IdMetodoPago}",
                    resultado);
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                _logger.LogWarning(
                    ex,
                    "Nombre duplicado al registrar método de pago. Empresa: {IdEmpresa}",
                    idEmpresa);

                return Conflict(new
                {
                    mensaje = "Ya existe un método de pago con ese nombre."
                });
            }
        }

        [HttpPut("{idMetodoPago:int}")]
        public async Task<IActionResult> Editar(
            [FromRoute] int idMetodoPago,
            [FromBody] MetodoPagoRequest metodo)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                await _metodoPagoFlujo.Editar(
                    idEmpresa: idEmpresa,
                    idUsuario: idUsuario,
                    idMetodoPago: idMetodoPago,
                    metodo: metodo);

                return NoContent();
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un método de pago con ese nombre."
                });
            }
        }

        [HttpPut("{idMetodoPago:int}/activar")]
        public async Task<IActionResult> Activar(
            [FromRoute] int idMetodoPago)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            await _metodoPagoFlujo.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idMetodoPago: idMetodoPago,
                activo: true);

            return NoContent();
        }

        [HttpPut("{idMetodoPago:int}/desactivar")]
        public async Task<IActionResult> Desactivar(
            [FromRoute] int idMetodoPago)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            await _metodoPagoFlujo.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idMetodoPago: idMetodoPago,
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
