using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [InventarioFisicoModelStateFilter]
    public class InventarioFisicoController : Controller
    {
        private IInventarioFisicoFlujo inventarioFisicoFlujo;

        public InventarioFisicoController(IInventarioFisicoFlujo inventarioFisicoFlujo)
        {
            this.inventarioFisicoFlujo = inventarioFisicoFlujo;
        }

        [HttpPost("RegistrarInventarioFisico")]
        public async Task<ActionResult> RegistrarInventarioFisico(InventarioFisico inventario)
        {
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

            var resultado = await inventarioFisicoFlujo.ObtenerExistencia(filtro);

            if (resultado == null)
            {
                return NotFound("No hay una existencia para ese producto y bodega");
            }

            return Ok(resultado);
        }
    }
}
