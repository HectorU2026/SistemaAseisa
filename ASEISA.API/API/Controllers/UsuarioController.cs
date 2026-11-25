using API.Seguridad;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ImpedirAutoadministracion]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioFlujo usuarioFlujo;
        private readonly IAdministracionCredenciales credenciales;

        public UsuarioController(IUsuarioFlujo usuarioFlujo, IAdministracionCredenciales credenciales)
        {
            this.usuarioFlujo = usuarioFlujo;
            this.credenciales = credenciales;
        }

        private bool ObtenerContexto(
            out int idEmpresa,
            out int idUsuarioAccion)
        {
            var empresaClaim = User.FindFirst("Empresa")?.Value;

            var usuarioClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            bool empresaValida = int.TryParse(
                empresaClaim, out idEmpresa);

            bool usuarioValido = int.TryParse(
                usuarioClaim, out idUsuarioAccion);

            return empresaValida && usuarioValido
                && idEmpresa > 0 && idUsuarioAccion > 0;
        }

        [HttpGet("ObtenerUsuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>>
            ObtenerUsuarios()
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out _))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            var usuarios = await usuarioFlujo.ObtenerUsuarios(
                idEmpresa);

            return Ok(usuarios);
        }

        [HttpGet("ObtenerUsuarioPorId/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioResponse>>
            ObtenerUsuarioPorId(int idUsuario)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out _))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
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

        [HttpPut("EditarUsuario/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> EditarUsuario(
            int idUsuario,
            [FromBody] UsuarioEditarRequest usuario)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            if (idUsuario <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            var actual=await usuarioFlujo.ObtenerUsuarioPorId(idEmpresa,idUsuario);
            if(actual==null)return NotFound();
            if(!string.Equals(actual.NombreUsuario,usuario.NombreUsuario,StringComparison.Ordinal))
                return BadRequest(new {codigo="USUARIO_INMUTABLE",mensaje="El nombre de usuario no puede modificarse."});
            var resultado = await usuarioFlujo.EditarUsuario(
                idEmpresa,
                idUsuario,
                usuario, idUsuarioAccion);

            return Ok(resultado);
        }

        [HttpPut("CambiarEstadoUsuario/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> CambiarEstadoUsuario(
            int idUsuario,
            [FromBody] UsuarioCambiarEstadoRequest solicitud)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            if (idUsuario <= 0 || solicitud.Activo == null)
            {
                return BadRequest(
                    "Debe indicar el usuario y el estado solicitado.");
            }

            var resultado = await usuarioFlujo.CambiarEstadoUsuario(
                idEmpresa,
                idUsuario,
                solicitud.Activo.Value,
                idUsuarioAccion);

            return Ok(resultado);
        }

        [HttpPost("AsignarRolUsuario/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> AsignarRolUsuario(
            int idUsuario,
            [FromBody] UsuarioAsignarRolRequest solicitud)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            if (idUsuario <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            var resultado = await usuarioFlujo.AsignarRolUsuario(
                idEmpresa,
                idUsuario,
                solicitud.IdRol,
                idUsuarioAccion);

            return Ok(resultado);
        }

        [HttpGet("ObtenerSolicitudesClientesPendientes")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<
            IEnumerable<SolicitudClientePendienteResponse>>>
            ObtenerSolicitudesClientesPendientes()
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            var solicitudes = await usuarioFlujo
                .ObtenerSolicitudesClientesPendientes(
                    idEmpresa,
                    idUsuarioAccion);

            return Ok(solicitudes);
        }

        [HttpPut("AprobarRegistroCliente/{idUsuario:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> AprobarRegistroCliente(
            int idUsuario,
            [FromBody] UsuarioAprobarSolicitudRequest solicitud)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            if (idUsuario <= 0 || solicitud.Aprobar == null)
            {
                return BadRequest(
                    "Debe indicar el usuario y la decisión.");
            }

            var resultado = await usuarioFlujo.AprobarRegistroCliente(
                idEmpresa,
                idUsuario,
                idUsuarioAccion,
                solicitud.Aprobar.Value);

            return Ok(resultado);
        }

        [HttpPost("AgregarUsuario")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> AgregarUsuario(
            [FromBody] UsuarioRequest usuario)
        {
            if (!ObtenerContexto(
                out int idEmpresa,
                out int idUsuarioAccion))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario o su empresa.");
            }

            var resultado=await credenciales.Registrar(usuario,HttpContext.RequestAborted);
            Response.Headers["X-Invitacion-Estado"]=resultado.EnvioProgramado ? "Programado" : "Pendiente";
            return Ok(resultado.IdUsuario);
        }
        [HttpDelete("RetirarRolUsuario/{idUsuario:int}/roles/{idRol:int}")]
        [Authorize(Roles="Administrador")]
        public async Task<IActionResult> RetirarRol(int idUsuario,int idRol)
        {
            if(!ObtenerContexto(out var empresa,out var actor)) return Unauthorized();
            if(idUsuario<=0 || idRol<=0) return BadRequest();
            var objetivo=await usuarioFlujo.ObtenerUsuarioPorId(empresa,idUsuario);
            if(objetivo==null)return NotFound();
            if(objetivo.Roles.Any(r=>r.IdRol==idRol) && objetivo.Roles.Select(r=>r.IdRol).Distinct().Count()<=1)
                return BadRequest(new {codigo="ULTIMO_ROL"});
            return Ok(await usuarioFlujo.RetirarRolUsuario(empresa,idUsuario,idRol,actor));
        }
        [HttpGet("Catalogos")]
        [Authorize(Roles="Administrador")]
        public async Task<IActionResult> Catalogos()
        {
            if(!ObtenerContexto(out var empresa,out var actor)) return Unauthorized();
            return Ok(await usuarioFlujo.ObtenerCatalogos(empresa,actor));
        }
        [HttpPost("RestablecerContrasenaUsuario/{idUsuario:int:min(1)}")]
        [Authorize(Roles="Administrador")]
        public async Task<IActionResult> Restablecer(int idUsuario)
            =>Ok(await credenciales.Restablecer(idUsuario,HttpContext.RequestAborted));
    }
}
