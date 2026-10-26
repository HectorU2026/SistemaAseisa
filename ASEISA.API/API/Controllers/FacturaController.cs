using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendedor,Contador")]
    public class FacturaController : Controller
    {
        private readonly IFacturaFlujo _facturaFlujo;

        public FacturaController(IFacturaFlujo facturaFlujo)
        {
            _facturaFlujo = facturaFlujo;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener([FromQuery] FacturaFiltro filtro)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.ObtenerFacturas(idUsuario, filtro);
        }

        [HttpGet("{idFactura:int}")]
        public async Task<IActionResult> ObtenerDetalle([FromRoute] int idFactura)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.ObtenerFactura(idUsuario, idFactura);
        }
    }
}
