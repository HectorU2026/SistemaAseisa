using Microsoft.Data.SqlClient;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class ContactoController : ControllerBase
    {
        private IContactoFlujo _contactoFlujo;
        private IUsuarioFlujo _usuarioFlujo;
        private ILogger<ContactoController> _logger;

        public ContactoController(
            IContactoFlujo contactoFlujo,
            IUsuarioFlujo usuarioFlujo,
            ILogger<ContactoController> logger)
        {
            _contactoFlujo = contactoFlujo;
            _usuarioFlujo = usuarioFlujo;
            _logger = logger;
        }

        #region "Operaciones"

        [HttpPost]
        public async Task<IActionResult> Agregar(
    [FromBody] ContactoRequest contacto)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                var resultado = await _contactoFlujo.Agregar(
                    idEmpresa, idUsuario, contacto);

                return CreatedAtAction(
                    nameof(ObtenerDetalle),
                    new { idContacto = resultado.IdContacto },
                    resultado);
            }
            catch (SqlException ex) when (
                ex.Message.Contains(
                    "Identificacion registrada",
                    StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Intento de registrar un contacto con identificación duplicada. " +
                    "Empresa: {IdEmpresa}. Usuario: {IdUsuario}.",
                    idEmpresa,
                    idUsuario);

                return Conflict(new
                {
                    mensaje = "La cédula o identificación fiscal ingresada ya está registrada en otro contacto."
                });
            }
        }

        [HttpPut("{idContacto:int}")]
        public async Task<IActionResult> Editar(
            [FromRoute] int idContacto,
            [FromBody] ContactoRequest contacto)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            if (!await VerificarContactoExiste(
                idEmpresa, idContacto, idUsuario))
            {
                return NotFound("El contacto no existe.");
            }

            try
            {
                var resultado = await _contactoFlujo.Editar(
                    idEmpresa,
                    idUsuario,
                    idContacto,
                    contacto);

                return Ok(resultado);
            }
            catch (SqlException ex) when (
                ex.Message.Contains(
                    "Identificacion registrada",
                    StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Identificación duplicada al editar contacto {IdContacto}. Empresa: {IdEmpresa}.",
                    idContacto,
                    idEmpresa);

                return Conflict(new
                {
                    mensaje = "La cédula o identificación fiscal ingresada ya está registrada en otro contacto."
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Obtener(
            [FromQuery] ContactoFiltro filtro)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            var resultado = await _contactoFlujo.Obtener(
                idEmpresa, filtro);

            if (!resultado.Any())
            {
                return NoContent();
            }

            return Ok(resultado);
        }

        [HttpGet("{idContacto:int}")]
        public async Task<IActionResult> ObtenerDetalle(
            [FromRoute] int idContacto)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            var resultado = await _contactoFlujo.Obtener(idEmpresa, idUsuario, idContacto);

            if (resultado == null)
            {
                return NotFound("El contacto no existe");
            }

            return Ok(resultado);
        }

        [HttpGet("catalogos-pago")]
        public async Task<IActionResult> ObtenerCatalogosPago()
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            var resultado = await _contactoFlujo.ObtenerCatalogosPago(
                idEmpresa, idUsuario);

            return Ok(resultado);
        }

        [HttpPut("{idContacto:int}/activar")]
        public async Task<IActionResult> Activar(
            [FromRoute] int idContacto)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            if (!await VerificarContactoExiste(idEmpresa, idContacto, idUsuario))
                return NotFound("El contacto no existe");

            var resultado = await _contactoFlujo.CambiarEstado(
                idEmpresa, idUsuario, idContacto, true);

            return Ok(resultado);
        }

        [HttpPut("{idContacto:int}/desactivar")]
        public async Task<IActionResult> Desactivar(
            [FromRoute] int idContacto)
        {
            var contexto = await ObtenerContextoUsuario();

            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;

            if (!await VerificarContactoExiste(idEmpresa, idContacto, idUsuario))
                return NotFound("El contacto no existe");

            var resultado = await _contactoFlujo.CambiarEstado(
                idEmpresa, idUsuario, idContacto, false);

            return Ok(resultado);
        }

        #endregion

        #region "Helpers"

        private async Task<bool> VerificarContactoExiste(
            int idEmpresa, int idContacto, int idUsuario)
        {
            var resultadoValidacion = false;
            var resultadoContactoExiste = await _contactoFlujo.Obtener(idEmpresa, idUsuario, idContacto);

            if (resultadoContactoExiste != null)
                resultadoValidacion = true;

            return resultadoValidacion;
        }

        private async Task<(int IdEmpresa, int IdUsuario)?> ObtenerContextoUsuario()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return null;

            var idEmpresa = await _usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return null;

            return (idEmpresa.Value, idUsuario);
        }

        #endregion
    }
}