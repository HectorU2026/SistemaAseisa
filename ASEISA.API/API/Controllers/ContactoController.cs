using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactoController : ControllerBase
    {
        private IContactoFlujo _contactoFlujo;
        private ILogger<ContactoController> _logger;

        public ContactoController(
            IContactoFlujo contactoFlujo,
            ILogger<ContactoController> logger)
        {
            _contactoFlujo = contactoFlujo;
            _logger = logger;
        }

        #region "Operaciones"

        [HttpPost]
        public async Task<IActionResult> Agregar(
            [FromQuery] int idEmpresa,
            [FromQuery] int idUsuario,
            [FromBody] ContactoRequest contacto)
        {
            var resultado = await _contactoFlujo.Agregar(
                idEmpresa, idUsuario, contacto);

            return CreatedAtAction(
                nameof(ObtenerDetalle),
                new
                {
                    idContacto = resultado.IdContacto,
                    idEmpresa = idEmpresa
                },
                resultado);
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(
            [FromQuery] int idEmpresa,
            [FromQuery] ContactoFiltro filtro)
        {
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
            [FromRoute] int idContacto,
            [FromQuery] int idEmpresa)
        {
            var resultado = await _contactoFlujo.Obtener(
                idEmpresa, idContacto);

            if (resultado == null)
            {
                return NotFound("El contacto no existe");
            }

            return Ok(resultado);
        }

        [HttpGet("catalogos-pago")]
        public async Task<IActionResult> ObtenerCatalogosPago(
            [FromQuery] int idEmpresa)
        {
            var resultado = await _contactoFlujo.ObtenerCatalogosPago(
                idEmpresa);

            return Ok(resultado);
        }

        #endregion
    }
}

