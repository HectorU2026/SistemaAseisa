using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendedor,Contador")]
    public class FacturaController : Controller
    {
        private readonly IFacturaFlujo _facturaFlujo;

        public FacturaController(IFacturaFlujo facturaFlujo)
        {
            _facturaFlujo = facturaFlujo;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener([FromQuery] FacturaFiltro filtro)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.ObtenerFacturas(idUsuario, filtro);
        }

        [HttpGet("{idFactura:int}")]
        public async Task<IActionResult> ObtenerDetalle([FromRoute] int idFactura)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.ObtenerFactura(idUsuario, idFactura);
        }

        [HttpPut("{idFactura:int}/confirmar")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> Confirmar(
            [FromRoute] int idFactura,
            [FromBody] FacturaCambioEstadoRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.Confirmar(idUsuario, idFactura, request.VersionFila);
        }

        [HttpPut("{idFactura:int}/restablecer")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> Restablecer(
            [FromRoute] int idFactura,
            [FromBody] FacturaCambioEstadoRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.Restablecer(idUsuario, idFactura, request.VersionFila);
        }

        [HttpPut("{idFactura:int}/inactivar")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> Inactivar(
            [FromRoute] int idFactura,
            [FromBody] FacturaInactivarRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.Inactivar(
                idUsuario,
                idFactura,
                request.VersionFila,
                request.MotivoAnulacion);
        }

        [HttpPut("{idFactura:int}/reactivar")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> Reactivar(
            [FromRoute] int idFactura,
            [FromBody] FacturaCambioEstadoRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.Reactivar(idUsuario, idFactura, request.VersionFila);
        }

        [HttpPost("{idFactura:int}/pagos")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> RegistrarPago(
            [FromRoute] int idFactura,
            [FromBody] FacturaPagoRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.RegistrarPago(
                idUsuario,
                idFactura,
                request.Monto,
                request.MetodoPago);
        }

        [HttpPost("{idFactura:int}/notas-credito")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> RegistrarNotaCredito(
            [FromRoute] int idFactura,
            [FromBody] FacturaNotaCreditoRequest request)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.RegistrarNotaCredito(
                idUsuario,
                idFactura,
                request.Motivo,
                request.Monto,
                request.DescripcionAdicional);
        }

        [HttpGet("{idFactura:int}/xml")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> DescargarXml([FromRoute] int idFactura)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.DescargarXml(idUsuario, idFactura);
        }

        [HttpPost("{idFactura:int}/xml/enviar")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> EnviarXml([FromRoute] int idFactura)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.EnviarXml(idUsuario, idFactura);
        }

        [HttpPost("{idFactura:int}/enviar")]
        [Authorize(Roles = "Contador")]
        public async Task<IActionResult> Enviar([FromRoute] int idFactura)
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) ||
                !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            return await _facturaFlujo.Enviar(idUsuario, idFactura);
        }
    }
}
