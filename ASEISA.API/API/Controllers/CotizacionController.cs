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
    }
}
