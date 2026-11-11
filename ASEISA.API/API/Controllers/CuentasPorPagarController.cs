using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Flujo;
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
        public async Task<ActionResult<IEnumerable<CuentaPorPagar>>> ObtenerCuentasPorPagar([FromQuery] int idEmpresa,string? busqueda = null,int? idEstado = null,DateTime? fechaVencimiento = null)
        {
            var resultado = await _cuentasPorPagarFlujo.ObtenerCuentasPorPagar(idEmpresa, busqueda, idEstado, fechaVencimiento);
            return Ok(resultado);
        }
    }
}
