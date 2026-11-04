using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioFlujo usuarioFlujo;

        public UsuarioController(IUsuarioFlujo usuarioFlujo)
        {
            this.usuarioFlujo = usuarioFlujo;
        }

        // Actualizar último acceso
        [HttpPost("ActualizarUltimoAcceso")]
        public async Task<ActionResult> ActualizarUltimoAcceso(
            string correo)
        {
            await usuarioFlujo.ActualizarUltimoAcceso(correo);
            return Ok();
        }

        // Obtener información del usuario por correo
        [HttpGet("ObtenerInfoUsuario")]
        public async Task<ActionResult<UsuarioResponse>> ObtenerInfoUsuario(
            string correo)
        {
            var infoUsuario =
                await usuarioFlujo.ObtenerInfoUsuario(correo);

            return Ok(infoUsuario);
        }

        // Obtener listado de usuarios de la empresa
        [HttpGet("ObtenerUsuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>> ObtenerUsuarios()
        {
            var idEmpresaClaim = User.FindFirst("idEmpresa")?.Value;

            if (!int.TryParse(idEmpresaClaim, out int idEmpresa))
            {
                return Unauthorized(
                    "No se pudo identificar la empresa del usuario.");
            }

            var usuarios =
                await usuarioFlujo.ObtenerUsuarios(idEmpresa);

            return Ok(usuarios);
        }

        // Obtener un usuario por ID
        [HttpGet("ObtenerUsuarioPorId/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioResponse>> ObtenerUsuarioPorId(
            int idUsuario)
        {
            var idEmpresaClaim = User.FindFirst("idEmpresa")?.Value;

            if (!int.TryParse(idEmpresaClaim, out int idEmpresa))
            {
                return Unauthorized(
                    "No se pudo identificar la empresa del usuario.");
            }

            if (idUsuario <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            var usuario = await usuarioFlujo.ObtenerUsuarioPorId(
                idEmpresa,
                idUsuario);

            if (usuario == null)
            {
                return NotFound(
                    "No se encontró el usuario solicitado.");
            }

            return Ok(usuario);
        }

        // Registrar usuario
        [HttpPost("AgregarUsuario")]
        public async Task<ActionResult<int>> AgregarUsuario(
            int idEmpresa,
            [FromBody] UsuarioRequest usuario)
        {
            var idUsuario =
                await usuarioFlujo.AgregarUsuario(idEmpresa, usuario);

            return Ok(idUsuario);
        }

        // Editar usuario
        [HttpPut("EditarUsuario")]
        public async Task<ActionResult<int>> EditarUsuario(
            int idEmpresa,
            int idUsuario,
            [FromBody] UsuarioRequest usuario)
        {
            var idUsuarioActualizado =
                await usuarioFlujo.EditarUsuario(
                    idEmpresa,
                    idUsuario,
                    usuario);

            return Ok(idUsuarioActualizado);
        }

        // Activar o desactivar usuario
        [HttpPut("CambiarEstadoUsuario")]
        public async Task<ActionResult<int>> CambiarEstadoUsuario(
            int idEmpresa,
            int idUsuario,
            int idEstado)
        {
            var idUsuarioActualizado =
                await usuarioFlujo.CambiarEstadoUsuario(
                    idEmpresa,
                    idUsuario,
                    idEstado);

            return Ok(idUsuarioActualizado);
        }

        // Restablecer contraseña
        [HttpPut("RestablecerContrasena")]
        public async Task<ActionResult<int>> RestablecerContrasenaUsuario(
            int idEmpresa,
            int idUsuario,
            string contrasenaHash)
        {
            var idUsuarioActualizado =
                await usuarioFlujo.RestablecerContrasenaUsuario(
                    idEmpresa,
                    idUsuario,
                    contrasenaHash);

            return Ok(idUsuarioActualizado);
        }

        [HttpPost("AsignarRolUsuario")]
        public async Task<ActionResult<int>> AsignarRolUsuario(
            int idEmpresa,
            int idUsuario,
            int idRol)
        {
            var idUsuarioActualizado =
                await usuarioFlujo.AsignarRolUsuario(
                    idEmpresa,
                    idUsuario,
                    idRol);

            return Ok(idUsuarioActualizado);
        }
    }
}
