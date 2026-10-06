using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProductoModelStateFilter]
    public class ProductoController : Controller
    {
        private IProductoFlujo productoFlujo;

        public ProductoController(IProductoFlujo productoFlujo)
        {
            this.productoFlujo = productoFlujo;
        }

        [HttpPost("RegistrarProducto")]
        public async Task<ActionResult> RegistrarProducto(Producto producto)
        {
            var resultado = await productoFlujo.RegistrarProducto(producto);

            if (resultado == 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (resultado == 2)
            {
                return BadRequest("Debe escribir un número positivo");
            }

            return Ok("Producto registrado con éxito");
        }
    }
}
