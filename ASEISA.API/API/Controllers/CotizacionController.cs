using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static Abstracciones.Modelos.CotizacionResponseDetallada;
using static Abstracciones.Modelos.Facturacion;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionFlujo _cotizacionFlujo;
        private readonly IUsuarioFlujo _usuarioFlujo;

        public CotizacionController(ICotizacionFlujo cotizacionFlujo, IUsuarioFlujo usuarioFlujo)
        {
            _cotizacionFlujo = cotizacionFlujo;
            _usuarioFlujo = usuarioFlujo;
        }

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

        [HttpPost("RegistrarCotizacionFlujo")]
        public async Task<IActionResult> RegistrarCotizacionFlujo([FromBody] CotizacionRequest cotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, idUsuario) = contexto.Value;
            cotizacion.IdEmpresa = idEmpresa;

            try
            {
                var result = await _cotizacionFlujo.RegistrarCotizacionFlujo(idEmpresa, idUsuario, cotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("AvanzarCotizacionEnviadaFlujo")]
        public async Task<IActionResult> AvanzarCotizacionEnviadaFlujo(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                var result = await _cotizacionFlujo.AvanzarCotizacionEnviadaFlujo(idEmpresa, idUsuario, idCotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerCotizacionPorId/{idCotizacion}")]
        public async Task<IActionResult> ObtenerCotizacionPorId(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                var result = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);

                if (result == null)
                    return NotFound("Cotización no encontrada.");

                if (result.IdEmpresa != idEmpresa)
                    return Forbid();

                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("EnviarCotizacionPorCorreo")]
        public async Task<IActionResult> EnviarCotizacionPorCorreo(int idCotizacion, [FromQuery] string correo)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                // Validación de pertenencia
                var cotizacion = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);
                if (cotizacion == null) return NotFound("Cotización no encontrada.");
                if (cotizacion.IdEmpresa != idEmpresa) return Forbid();

                var result = await _cotizacionFlujo.EnviarCotizacionPorCorreo(idCotizacion, correo);
                return result;
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("RetrocederCotizacionEnviada")]
        public async Task<IActionResult> RetrocederCotizacionEnviada(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                var cotizacion = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);
                if (cotizacion == null) return NotFound("Cotización no encontrada.");
                if (cotizacion.IdEmpresa != idEmpresa) return Forbid();

                var result = await _cotizacionFlujo.RetrocederCotizacionEnviada(idCotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("AvanzarCotizacionAOrdenDeVenta")]
        public async Task<IActionResult> AvanzarCotizacionAOrdenDeVenta(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                var cotizacion = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);
                if (cotizacion == null) return NotFound("Cotización no encontrada.");
                if (cotizacion.IdEmpresa != idEmpresa) return Forbid();

                var result = await _cotizacionFlujo.AvanzarCotizacionAOrdenDeVenta(idCotizacion);
                return result;
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // FAC-001: Crear factura a partir de una orden de venta
        [Authorize(Roles = "Administrador,Vendedor,Contador")]
        [HttpPost("FacturarOrdenDeVenta/{idCotizacion}")]
        public async Task<IActionResult> Facturar(int idCotizacion, [FromBody] FacturarRequest request)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (_, idUsuario) = contexto.Value;

            try
            {
                var resultado = await _cotizacionFlujo.Facturar(idCotizacion, idUsuario, request);
                return resultado;
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerCanceladasRechazadas")]
        public async Task<IActionResult> ObtenerCanceladasRechazadas(
            [FromQuery] string? busqueda = null,
            [FromQuery] DateTime? fecha = null)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                return await _cotizacionFlujo.ObtenerCanceladasRechazadas(idEmpresa, busqueda, fecha);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerCotizacionesFlujo")]
        public async Task<ActionResult> ObtenerCotizaciones([FromQuery] CotizacionFiltro filtro)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            filtro ??= new CotizacionFiltro();
            filtro.IdEmpresa = idEmpresa;

            try
            {
                var resultado = await _cotizacionFlujo.ObtenerCotizacionesFlujo(filtro);
                return Ok(resultado);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("EditarCotizacionFlujo")]
        public async Task<IActionResult> EditarCotizacionFlujo([FromBody] CotizacionRequest cotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, idUsuario) = contexto.Value;

            if (cotizacion == null) return BadRequest("El cuerpo de la solicitud es obligatorio.");
            cotizacion.IdEmpresa = idEmpresa;

            try
            {
                var result = await _cotizacionFlujo.EditarCotizacionFlujo(idEmpresa, idUsuario, cotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("CancelarCotizacionFlujo")]
        public async Task<IActionResult> CancelarCotizacionFlujo(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, idUsuario) = contexto.Value;

            try
            {
                var result = await _cotizacionFlujo.CancelarCotizacionFlujo(idEmpresa, idUsuario, idCotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("RetrocederAEnviada")]
        public async Task<IActionResult> RetrocederAEnviada(int idCotizacion)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) 
                return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            try
            {
                var cotizacion = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);
                if (cotizacion == null) return NotFound("Cotización no encontrada.");
                if (cotizacion.IdEmpresa != idEmpresa) return Forbid();

                var result = await _cotizacionFlujo.RetrocederAEnviada(idCotizacion);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}