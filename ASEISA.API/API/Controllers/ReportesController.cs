using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Flujo;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : Controller
    {
        private readonly IUsuarioFlujo _usuarioFlujo;
        private IReportesFlujo _reportesFlujo;


        public ReportesController(IReportesFlujo reportesFlujo, IUsuarioFlujo usuarioFlujo)
        {
            _usuarioFlujo = usuarioFlujo;
            _reportesFlujo = reportesFlujo;
        }

        [HttpGet("ObtenerBalanceGeneral")]
        public async Task<IActionResult> ObtenerBalanceGeneral([FromQuery] BalanceGeneralFiltro filtro)
        {
            var contexto = await ObtenerContextoUsuario();
            if (!contexto.HasValue)
                return Unauthorized("Usuario no registrado");

            var resultado = await _reportesFlujo.ObtenerBalanceGeneral(contexto.Value.IdEmpresa, filtro);
            if (resultado == null)
            {
                return NotFound("No se encontraron resultados para el balance general");
            }
            return Ok(resultado);
        }


        [HttpGet("ObtenerLibroMayor")]
        public async Task<IActionResult> ObtenerLibroMayor([FromQuery] LibroMayorRequest request)
        {
            var contexto = await ObtenerContextoUsuario();
            if (!contexto.HasValue)
                return Unauthorized("Usuario no registrado");

            var resultado = await _reportesFlujo.ObtenerLibroMayor(contexto.Value.IdEmpresa, request);
            if (resultado == null)
            {
                return NotFound("No se encontraron resultados para el libro mayor");
            }
            return Ok(resultado);
        }

        [HttpGet("ObtenerReporteCuentaPorPagar")]
        public async Task<IActionResult> ObtenerReporteCuentaPorPagar([FromQuery] ReporteCuentaPorPagarRequest request)
        {
            var contexto = await ObtenerContextoUsuario();
            if (!contexto.HasValue)
                return Unauthorized("Usuario no registrado");

            var resultado = await _reportesFlujo.ObtenerReporteCuentaPorPagar(contexto.Value.IdEmpresa, request);
            if (resultado == null)
            {
                return NotFound("No se encontraron resultados para el reporte de cuentas por pagar");
            }
            return Ok(resultado);
        }


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
        #endregion
    }
}
