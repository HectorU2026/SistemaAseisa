using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasPorPagarController : Controller
    {
        private ICuentasPorPagarFlujo _cuentasPorPagarFlujo;

        public CuentasPorPagarController(ICuentasPorPagarFlujo cuentasPorPagarFlujo)
        {
            _cuentasPorPagarFlujo = cuentasPorPagarFlujo;
        }


        [HttpGet("ObtenerCuentasPorPagar")]
        public async Task<IActionResult> ObtenerCuentasPorPagar()
        {
            var contexto = await _cuentasPorPagarFlujo.ObtenerCuentasPorPagar();

            if (!contexto.Any())
            {
                return NoContent();
            }

            return Ok(contexto);
        }
    }
}
