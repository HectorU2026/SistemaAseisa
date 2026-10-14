using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionController : Controller
    {
        private ICotizacionFlujo cotizacionFlujo;

        public CotizacionController(ICotizacionFlujo cotizacionFlujo)
        {
            this.cotizacionFlujo = cotizacionFlujo;
        }

        [HttpPost("RegistrarCotizacionFlujo")]
        public async Task<IActionResult> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, [FromBody] CotizacionRequest cotizacion)
        {
            var result = await cotizacionFlujo.RegistrarCotizacionFlujo(idEmpresa, idUsuario, cotizacion);
            return Ok(result);
        }

        [HttpPut("AvanzarCotizacionEnviadaFlujo")]
        public async Task<IActionResult> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            var result = await cotizacionFlujo.AvanzarCotizacionEnviadaFlujo(idEmpresa, idUsuario, idCotizacion);
            return Ok(result);
        }
    }
}
