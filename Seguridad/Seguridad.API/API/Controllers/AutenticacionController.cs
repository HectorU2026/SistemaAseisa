using Abstracciones.API;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : Controller, IAutenticacionController
    {
        private IAutenticacionFlujo _autenticacionFlujo;

        public AutenticacionController(IAutenticacionFlujo autenticacionFlujo)
        {
            _autenticacionFlujo = autenticacionFlujo;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> PostAsync([FromBody] LoginBase login)
        {
            return Ok(await _autenticacionFlujo.LoginAsync(login));
        }

        [AllowAnonymous]
        [HttpPost("recuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasena([FromBody] UsuarioRecuperar correo) {
            return Ok(await _autenticacionFlujo.RecuperarContrasena(correo));
        
        }

        [AllowAnonymous]
        [HttpPost("cambiarContrasena")]
        public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasena usuario)
        {
            return Ok(await _autenticacionFlujo.CambiarContrasena(usuario));

        }


    }
}
