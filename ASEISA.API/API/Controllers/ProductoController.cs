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
    [ProductoModelStateFilter]
    public class ProductoController : Controller
    {
        private IProductoFlujo productoFlujo;
        private IUsuarioFlujo usuarioFlujo;

        public ProductoController(IProductoFlujo productoFlujo, IUsuarioFlujo usuarioFlujo)
        {
            this.productoFlujo = productoFlujo;
            this.usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("RegistrarProducto")]
        [Authorize(Roles = "Administrador,Contador,Comprador")]
        public async Task<ActionResult> RegistrarProducto(Producto producto)
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return Unauthorized();

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return Forbid();

            if (producto == null
                || !producto.IdEmpresa.HasValue
                || producto.IdEmpresa <= 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (producto.IdEmpresa.Value != idEmpresa.Value)
                return Forbid();

            var resultado = await productoFlujo.RegistrarProducto(producto, idEmpresa.Value);

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
        [Authorize(Roles = "Administrador,Contador,Comprador")]
        public async Task<ActionResult> EditarProducto(Producto producto)
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return Unauthorized();

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return Forbid();

            var resultado = await productoFlujo.EditarProducto(producto, idEmpresa.Value);

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
        [Authorize(Roles = "Administrador,Contador")]
        public async Task<ActionResult> InhabilitarProducto(ProductoInhabilitar producto)
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return Unauthorized();

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return Forbid();

            if (producto == null
                || !producto.IdProducto.HasValue
                || producto.IdProducto <= 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            var resultado = await productoFlujo.InhabilitarProducto(producto, idEmpresa.Value);

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
        [Authorize(Roles = "Administrador,Contador,Auxiliar,Comprador,Vendedor")]
        public async Task<ActionResult<ProductoCatalogoRespuesta>> ObtenerProductos([FromQuery] ProductoFiltro filtro)
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return Unauthorized();

            var idEmpresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return Forbid();

            if (filtro == null
                || !filtro.IdEmpresa.HasValue
                || filtro.IdEmpresa <= 0)
            {
                return BadRequest("Complete todos los campos primero");
            }

            if (filtro.IdEmpresa.Value != idEmpresa.Value)
                return Forbid();

            var resultado = await productoFlujo.ObtenerProductos(filtro);
            return Ok(resultado);
        }
    }
}
