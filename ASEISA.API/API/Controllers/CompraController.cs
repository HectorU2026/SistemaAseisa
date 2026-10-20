using System.Security.Claims;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Contador,Auxiliar")]
    [FiltroErroresCompra]

    public class CompraController : ControllerBase
    {
        private readonly ICompraFlujo _compraFlujo;
        private readonly IUsuarioFlujo _usuarioFlujo;

        public CompraController(ICompraFlujo compraFlujo, IUsuarioFlujo usuarioFlujo)
        {
            _compraFlujo = compraFlujo;
            _usuarioFlujo = usuarioFlujo;
        }

        #region "Operaciones"

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] CompraRequest compra)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.Agregar(idEmpresa, idUsuario, compra);

            return CreatedAtAction(nameof(ObtenerDetalle),
                new { idCompra = resultado.IdCompra }, resultado);
        }

        [HttpPut("{idCompra:int}")]
        public async Task<IActionResult> Editar(
            [FromRoute] int idCompra, [FromBody] CompraRequest compra)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.Editar(idEmpresa, idUsuario, idCompra, compra);

            return Ok(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> Obtener([FromQuery] CompraFiltro filtro)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.Obtener(idEmpresa, idUsuario, filtro);

            return Ok(resultado);
        }

        [HttpGet("{idCompra:int}")]
        public async Task<IActionResult> ObtenerDetalle([FromRoute] int idCompra)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.Obtener(idEmpresa, idUsuario, idCompra);

            if (resultado == null)
                return NotFound("La compra no existe");

            return Ok(resultado);
        }

        [HttpGet("catalogos")]
        public async Task<IActionResult> ObtenerCatalogos()
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.ObtenerCatalogos(idEmpresa, idUsuario);

            return Ok(resultado);
        }

        [HttpPut("{idCompra:int}/anular")]
        [Authorize(Roles = "Administrador,Contador")]
        public async Task<IActionResult> Anular([FromRoute] int idCompra)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null)
                return Forbid();

            var (idEmpresa, idUsuario) = contexto.Value;
            var resultado = await _compraFlujo.Anular(idEmpresa, idUsuario, idCompra);

            return Ok(resultado);
        }

        #endregion

        #region "Helpers"

        private async Task<(int IdEmpresa, int IdUsuario)?> ObtenerContextoUsuario()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                return null;

            var idEmpresa = await _usuarioFlujo.ObtenerEmpresaUsuarioActivo(idUsuario);
            if (!idEmpresa.HasValue || idEmpresa.Value <= 0)
                return null;

            return (idEmpresa.Value, idUsuario);
        }

        public class FiltroErroresCompraAttribute : ExceptionFilterAttribute
        {
            public override void OnException(ExceptionContext context)
            {
                if (context.Exception is ValidationException validacion)
                {
                    context.Result = new BadRequestObjectResult(
                        validacion.Message);

                    context.ExceptionHandled = true;
                    return;
                }

                if (context.Exception is not SqlException error)
                    return;

                int? codigoHttp = error.Number switch
                {
                    51201 => StatusCodes.Status403Forbidden,

                    51209 => StatusCodes.Status404NotFound,

                    51210 or 51211 => StatusCodes.Status409Conflict,

                    >= 51202 and <= 51208
                        => StatusCodes.Status400BadRequest,

                    >= 51213 and <= 51218
                        => StatusCodes.Status400BadRequest,

                    51220 => StatusCodes.Status400BadRequest,

                    _ => null
                };

                if (!codigoHttp.HasValue)
                    return;

                context.Result = new ObjectResult(error.Message)
                {
                    StatusCode = codigoHttp.Value
                };

                context.ExceptionHandled = true;
            }
        }

        #endregion
    }
}
