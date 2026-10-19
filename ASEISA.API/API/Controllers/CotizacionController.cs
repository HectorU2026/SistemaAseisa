using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Abstracciones.Modelos.Facturacion;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionController : Controller
    {
        private ICotizacionFlujo _cotizacionFlujo;

        public CotizacionController(ICotizacionFlujo cotizacionFlujo)
        {
            _cotizacionFlujo = cotizacionFlujo;
        }

        [HttpPost("RegistrarCotizacionFlujo")]
        public async Task<IActionResult> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, [FromBody] CotizacionRequest cotizacion)
        {
            var result = await _cotizacionFlujo.RegistrarCotizacionFlujo(idEmpresa, idUsuario, cotizacion);
            return Ok(result);
        }

        [HttpPut("AvanzarCotizacionEnviadaFlujo")]
        public async Task<IActionResult> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            var result = await _cotizacionFlujo.AvanzarCotizacionEnviadaFlujo(idEmpresa, idUsuario, idCotizacion);
            return Ok(result);
        }

        [HttpGet("ObtenerCotizacionPorId/{idCotizacion}")]
        public async Task<IActionResult> ObtenerCotizacionPorId(int idCotizacion)
        {
            var result = await _cotizacionFlujo.ObtenerCotizacionPorId(idCotizacion);
            return Ok(result);
        }
        
        [HttpPut("EnviarCotizacionPorCorreo")]
        public async Task<IActionResult> EnviarCotizacionPorCorreo(int idCotizacion, string correo)
        {
            var result = await _cotizacionFlujo.EnviarCotizacionPorCorreo(idCotizacion, correo);
            return Ok(result);
        }

        [HttpPut("RetrocederCotizacionEnviada")]
        public async Task<IActionResult> RetrocederCotizacionEnviada(int idCotizacion)
        {
            var result = await _cotizacionFlujo.RetrocederCotizacionEnviada(idCotizacion);
            return Ok(result);
        }

        [HttpPut("AvanzarCotizacionAOrdenDeVenta")]
        public async Task<IActionResult> AvanzarCotizacionAOrdenDeVenta(int idCotizacion)
        {
            var result = await _cotizacionFlujo.AvanzarCotizacionAOrdenDeVenta(idCotizacion);
            return Ok(result);
        }

        [HttpPost("FacturarOrdenDeVenta/{idCotizacion}")]
        public async Task<IActionResult> Facturar(int idCotizacion, [FromBody] FacturarRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var resultado = await _cotizacionFlujo.Facturar(idCotizacion, idUsuario, request);


            return resultado;
        }

        [HttpGet("ObtenerCanceladasRechazadas")]
        public async Task<IActionResult> ObtenerCanceladasRechazadas()
        {
            return await _cotizacionFlujo.ObtenerCanceladasRechazadas();
        }
    }
}


