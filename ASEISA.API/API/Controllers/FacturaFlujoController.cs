using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/Factura/flujo")]
    [ApiController]
    [Authorize]
    public class FacturaFlujoController : Controller
    {
        private readonly IFacturaFlujo _facturaFlujo;

        public FacturaFlujoController(IFacturaFlujo facturaFlujo)
        {
            _facturaFlujo = facturaFlujo;
        }

        [HttpGet]
        [ProducesResponseType(typeof(FlujoFacturaConsulta), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Obtener([FromQuery] FlujoFacturaFiltro filtro)
        {
            var acceso = ValidarAcceso();
            if (acceso != null)
                return acceso;

            var idUsuario = ObtenerIdUsuario();
            if (!idUsuario.HasValue)
                return Unauthorized("No se pudo identificar al usuario.");

            return await _facturaFlujo.ObtenerFlujoFacturas(idUsuario.Value, filtro);
        }

        [HttpGet("{idFactura:int}")]
        [ProducesResponseType(typeof(FlujoFacturaDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerDetalle([FromRoute] int idFactura)
        {
            var acceso = ValidarAcceso();
            if (acceso != null)
                return acceso;

            var idUsuario = ObtenerIdUsuario();
            if (!idUsuario.HasValue)
                return Unauthorized("No se pudo identificar al usuario.");

            return await _facturaFlujo.ObtenerFlujoFactura(idUsuario.Value, idFactura);
        }

        private IActionResult? ValidarAcceso()
        {
            if (User.IsInRole("Vendedor") || User.IsInRole("Contador"))
                return null;

            return StatusCode(StatusCodes.Status403Forbidden, "Acceso denegado");
        }

        private int? ObtenerIdUsuario()
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out int idUsuario))
                return null;

            return idUsuario;
        }
    }
}
