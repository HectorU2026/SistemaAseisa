using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BodegaController : Controller
    {
        private IBodegaFlujo bodegaFlujo;

        public BodegaController(IBodegaFlujo bodegaFlujo)
        {
            this.bodegaFlujo = bodegaFlujo;
        }

        [HttpPost("RegistrarBodega")]
        public async Task<ActionResult> RegistrarBodega(Bodega bodega)
        {
            var resultado = await bodegaFlujo.RegistrarBodega(bodega);

            if (resultado == 0)
            {
                return BadRequest("Debe rellenar todos los campos");
            }

            if (resultado == -1)
            {
                return BadRequest("Bodega ya existe, revise su nombre o ubicación");
            }

            return Ok("Bodega registrada con éxito");
        }
    }
}
