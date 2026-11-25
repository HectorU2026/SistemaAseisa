using Abstracciones.API;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller, IUsuarioController
    {
        private IUsuarioFlujo _usuarioFlujo;

        public UsuarioController(IUsuarioFlujo usuarioFlujo)
        {
            _usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("ObtenerInformacionUsuario")]
        public async Task<IActionResult> ObtenerUsuario([FromBody] InformacionUsuarioRequest solicitud)
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario)
                || idUsuario <= 0 || string.IsNullOrWhiteSpace(correo)
                || !int.TryParse(User.FindFirst("Empresa")?.Value, out var idEmpresa) || idEmpresa <= 0)
                return Unauthorized();

            if (!string.Equals(solicitud.Correo.Trim(), correo, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var usuario = await _usuarioFlujo.ObtenerUsuario(new LoginBase
            {
                Correo = correo,
                ContrasenaHash = string.Empty
            });
            if (usuario == null || usuario.IdUsuario != idUsuario
                || !int.TryParse(usuario.IdEmpresa, out var empresaActual)
                || empresaActual != idEmpresa
                || !string.Equals(usuario.Correo, correo, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return Ok(new InformacionUsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo,
                IdEmpresa = usuario.IdEmpresa,
                Rol = usuario.rol
            });
        }

        [AllowAnonymous]
        [HttpPost("RegistrarUsuario")]
        public async Task<IActionResult> PostAsync([FromBody] RegistroRequest infoUsuario)
        {
            return Ok(await _usuarioFlujo.CrearUsuario(infoUsuario));
        }








    }
}
