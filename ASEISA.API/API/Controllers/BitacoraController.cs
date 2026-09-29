using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController : Controller
    {
        private IBitacoraFlujo bitacoraFlujo;

        public BitacoraController(IBitacoraFlujo bitacoraFlujo)
        {
            this.bitacoraFlujo = bitacoraFlujo;
        }

        [HttpPost("RegistrarBitacora")]
        public async Task<ActionResult> RegistrarBitacora(Bitacora bitacora)
        {
            await bitacoraFlujo.RegistrarBitacora(bitacora);
            return Ok();
        }
    }
}
