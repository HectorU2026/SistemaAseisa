using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [IngresoProductoModelStateFilter]
    public class IngresoProductoController : Controller
    {
        private IIngresoProductoFlujo ingresoProductoFlujo;

        public IngresoProductoController(IIngresoProductoFlujo ingresoProductoFlujo)
        {
            this.ingresoProductoFlujo = ingresoProductoFlujo;
        }

        [HttpPost("RegistrarIngresoProducto")]
        public async Task<ActionResult> RegistrarIngresoProducto(IngresoProducto ingreso)
        {
            var resultado = await ingresoProductoFlujo.RegistrarIngresoProducto(ingreso);

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
                return BadRequest("Bodega, usuario o contacto inválido para la empresa");
            }

            if (resultado == 5)
            {
                return BadRequest("No hay una existencia para ese producto y bodega");
            }

            if (resultado == 8)
            {
                return BadRequest("Los estados requeridos no están configurados correctamente");
            }

            if (resultado == 9)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No se pudo actualizar la existencia durante la recepción");
            }

            if (resultado == 10)
            {
                return BadRequest("No hay existencias suficientes");
            }

            if (resultado == 11)
            {
                return BadRequest("Tipo de operación o estado no permitido");
            }

            if (resultado == 12)
            {
                return BadRequest("La cotización de venta no es válida para la empresa");
            }

            if (resultado == 13)
            {
                return BadRequest("La distribución analítica no es válida para la empresa");
            }

            if (resultado != 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No se pudo registrar el ingreso de producto");
            }

            return Ok("Ingreso de producto registrado con éxito");
        }
    }
}
