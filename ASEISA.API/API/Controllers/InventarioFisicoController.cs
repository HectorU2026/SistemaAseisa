using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Contador,Comprador")]
    [InventarioFisicoModelStateFilter]
    public class InventarioFisicoController : Controller
    {
        private IInventarioFisicoFlujo inventarioFisicoFlujo;
        private IUsuarioFlujo usuarioFlujo;

        public InventarioFisicoController(IInventarioFisicoFlujo inventarioFisicoFlujo, IUsuarioFlujo usuarioFlujo)
        {
            this.inventarioFisicoFlujo = inventarioFisicoFlujo;
            this.usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("RegistrarInventarioFisico")]
        public async Task<ActionResult> RegistrarInventarioFisico(InventarioFisico inventario)
        {
            var identidad = await ObtenerIdentidadEmpresaActiva();
            if (identidad.Result != null)
                return identidad.Result;

            if (inventario == null)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (inventario.IdEmpresa.HasValue
                && inventario.IdEmpresa.Value != identidad.IdEmpresa)
            {
                return Forbid();
            }

            if (inventario.IdUsuario.HasValue
                && inventario.IdUsuario.Value != identidad.IdUsuario)
            {
                return Forbid();
            }

            inventario.IdEmpresa = identidad.IdEmpresa;
            inventario.IdUsuario = identidad.IdUsuario;

            var resultado = await inventarioFisicoFlujo.RegistrarInventarioFisico(inventario);

            if (resultado == 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (resultado == 2)
            {
                return BadRequest("Debe escribir un número positivo");
            }

            if (resultado == 3)
            {
                return NotFound("El producto no existe");
            }

            if (resultado == 4)
            {
                return BadRequest("La bodega o el usuario no son válidos para la empresa");
            }

            if (resultado == 5)
            {
                return BadRequest("No hay una existencia para ese producto y bodega");
            }

            if (resultado == 8)
            {
                return BadRequest("El catálogo ESTADO debe tener un único Activo");
            }

            if (resultado == 9)
            {
                return Conflict("Las existencias cambiaron; debe revisar el conteo");
            }

            if (resultado != 1)
            {
                return BadRequest("Complete todos los campos primero");
            }

            return Ok("Registrado correctamente");
        }

        [HttpGet("ObtenerExistencia")]
        public async Task<ActionResult<ExistenciaConsulta>> ObtenerExistencia([FromQuery] ExistenciaFiltro filtro)
        {
            var identidad = await ObtenerIdentidadEmpresaActiva();
            if (identidad.Result != null)
                return identidad.Result;

            if (filtro == null
                || !filtro.IdEmpresa.HasValue
                || filtro.IdEmpresa <= 0
                || !filtro.IdProducto.HasValue
                || filtro.IdProducto <= 0
                || !filtro.IdBodega.HasValue
                || filtro.IdBodega <= 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (filtro.IdEmpresa.Value != identidad.IdEmpresa)
            {
                return Forbid();
            }

            var resultado = await inventarioFisicoFlujo.ObtenerExistencia(filtro);

            if (resultado == null)
            {
                return NotFound("No hay una existencia para ese producto y bodega");
            }

            return Ok(resultado);
        }

        private async Task<(ActionResult? Result, int IdUsuario, int IdEmpresa)> ObtenerIdentidadEmpresaActiva()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return (Unauthorized(), 0, 0);

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return (Forbid(), 0, 0);

            return (null, idUsuario, idEmpresa.Value);
        }
    }
}
