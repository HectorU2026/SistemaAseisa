using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {
        private IUsuarioFlujo usuarioFlujo;

        public UsuarioController(IUsuarioFlujo usuarioFlujo)
        {
            this.usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("ActualizarUltimoAcceso")]
        public async Task<ActionResult> ActualizarUltimoAcceso(string correo)
        {
            await usuarioFlujo.ActualizarUltimoAcceso(correo);
            return Ok();
        }

        [HttpPost("ObtenerInfoUsuario")]
        public async Task<ActionResult<Usuario>> ObtenerInfoUsuario(string correo)
        {
            var infoUsuario = await usuarioFlujo.ObtenerInfoUsuario(correo);
            return Ok(infoUsuario);
        }
    }
}
