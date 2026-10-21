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

        [HttpPost("EditarProducto")]
        public async Task<ActionResult> EditarProducto(Producto producto)
        {
            var resultado = await productoFlujo.EditarProducto(producto);

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
                return BadRequest("La bodega, las cuentas o el estado no son válidos para la empresa del producto");
            }

            if (resultado == 5)
            {
                return BadRequest("No hay una existencia de origen para cambiar la bodega");
            }

            if (resultado == 6)
            {
                return BadRequest("El cambio de bodega es ambiguo porque el producto tiene existencias en varias bodegas");
            }

            if (resultado == 7)
            {
                return BadRequest("Ya existe una existencia del producto en la bodega destino");
            }

            if (resultado != 1)
            {
                return BadRequest("Complete todos los campos primero");
            }

            return Ok("Producto editado con éxito");
        }

        [HttpPost("InhabilitarProducto")]
        public async Task<ActionResult> InhabilitarProducto(ProductoInhabilitar producto)
        {
            var resultado = await productoFlujo.InhabilitarProducto(producto);

            if (resultado == 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (resultado == 3)
            {
                return NotFound("El producto no existe");
            }

            if (resultado == 8)
            {
                return BadRequest("El catálogo ESTADO debe tener un único Activo y un único Inactivo");
            }

            if (resultado != 1)
            {
                return BadRequest("Complete todos los campos primero");
            }

            return Ok("Producto inhabilitado con éxito");
        }

        [HttpGet("ObtenerProductos")]
        public async Task<ActionResult<ProductoCatalogoRespuesta>> ObtenerProductos([FromQuery] ProductoFiltro filtro)
        {
            if (filtro == null
                || !filtro.IdEmpresa.HasValue
                || filtro.IdEmpresa <= 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            var resultado = await productoFlujo.ObtenerProductos(filtro);
            return Ok(resultado);
        }
    }
}
