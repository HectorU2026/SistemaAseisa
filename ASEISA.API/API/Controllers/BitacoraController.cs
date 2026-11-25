using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController : Controller
    {
        // Se conserva el constructor para compatibilidad. Las operaciones internas
        // siguen utilizando IBitacoraDA desde sus flujos, sin pasar por este endpoint.
        public BitacoraController(IBitacoraFlujo bitacoraFlujo)
        {
        }

        [HttpPost("RegistrarBitacora")]
        public Task<ActionResult> RegistrarBitacora(Bitacora bitacora)
        {
            // Un JWT no demuestra que el evento enviado haya ocurrido.
            // Ningun cliente puede escribir auditorias arbitrarias, incluso con rol administrador.
            return Task.FromResult<ActionResult>(Forbid());
        }
    }
}
