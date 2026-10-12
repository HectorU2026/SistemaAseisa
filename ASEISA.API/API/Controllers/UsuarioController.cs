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
         
        [HttpGet("ObtenerInfoUsuario")]
        public async Task<ActionResult<UsuarioResponse>> ObtenerInfoUsuario(string correo)
        {
            var infoUsuario = await usuarioFlujo.ObtenerInfoUsuario(correo);
            return Ok(infoUsuario);
        }

        [HttpPost("AgregarUsuario")]
        public async Task<ActionResult<int>> AgregarUsuario(int idEmpresa, [FromBody] UsuarioRequest usuario)
        {
            var idUsuario = await usuarioFlujo.AgregarUsuario(idEmpresa, usuario);
            return Ok(idUsuario);
        }
        
        [HttpPut("EditarUsuario")]
        public async Task<ActionResult<int>> EditarUsuario(int idEmpresa, int idUsuario, [FromBody] UsuarioRequest usuario)
        {
            var idUsuarioActualizado = await usuarioFlujo.EditarUsuario(idEmpresa, idUsuario, usuario);
            return Ok(idUsuarioActualizado);
        }

    }
}
