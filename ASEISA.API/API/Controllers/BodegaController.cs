using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BodegaController : Controller
    {
        private IBodegaFlujo bodegaFlujo;
        private IUsuarioFlujo usuarioFlujo;

        public BodegaController(IBodegaFlujo bodegaFlujo, IUsuarioFlujo usuarioFlujo)
        {
            this.bodegaFlujo = bodegaFlujo;
            this.usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("RegistrarBodega")]
        [Authorize(Roles = "Administrador,Contador,Comprador")]
        public async Task<ActionResult> RegistrarBodega(Bodega bodega)
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return Unauthorized();

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return Forbid();

            if (bodega == null)
                return BadRequest("Debe rellenar todos los campos");

            bodega.IdEmpresa = idEmpresa.Value;

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
