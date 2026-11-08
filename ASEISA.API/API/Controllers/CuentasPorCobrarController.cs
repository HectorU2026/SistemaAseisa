using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasPorCobrarController : ControllerBase
    {
        private readonly ICuentaPorCobrarFlujo _cuentaPorCobrarFlujo;

        public CuentasPorCobrarController(ICuentaPorCobrarFlujo cuentaPorCobrarFlujo)
        {
            _cuentaPorCobrarFlujo = cuentaPorCobrarFlujo;
        }

        [HttpGet("empresa/{idEmpresa}/usuario/{idUsuario}")]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrarResponse>>> ObtenerCuentasPorCobrar(int idEmpresa, int idUsuario)
        {
            if (idEmpresa <= 0 || idUsuario <= 0)
            {
                return BadRequest("Los identificadores de empresa y usuario son inválidos.");
            }

            var resultado = await _cuentaPorCobrarFlujo.ObtenerCuentasPorCobrar(idEmpresa, idUsuario);
            return Ok(resultado);
        }
    }
}
