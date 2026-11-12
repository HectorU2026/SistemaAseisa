using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Flujo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BancoController : Controller
    {
        private IBancoFlujo _bancoFlujo;
        private IUsuarioFlujo _usuarioFlujo;

        public BancoController(IBancoFlujo bancoFlujo, IUsuarioFlujo usuarioFlujo)
        {
            _bancoFlujo = bancoFlujo;
            _usuarioFlujo = usuarioFlujo;
        }

        [HttpPost("RegistrarCuentaBancaria")]
        public async Task<ActionResult> RegistrarCuentaBancaria(CuentaBancariaRequest cuentaBancaria)
        {
            cuentaBancaria.IdEstado = 1;
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;
            cuentaBancaria.IdEmpresa = idEmpresa;

            await _bancoFlujo.RegistrarCuentaBancaria(cuentaBancaria);
            return Ok();
        }

        [HttpGet("ObtenerCuentasBancarias")]
        public async Task<ActionResult> ObtenerCuentasBancarias()
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;
            return await _bancoFlujo.ObtenerCuentasBancarias(idEmpresa);
        }

        [HttpGet("ObtenerTiposCuenta")]
        public async Task<ActionResult> ObtenerTiposCuenta()
        {
            return await _bancoFlujo.ObtenerTiposCuenta();
        }

        [HttpGet("ObtenerCuentasContables")]
        public async Task<ActionResult> ObtenerCuentasContables()
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;
            return await _bancoFlujo.ObtenerCuentasContables(idEmpresa);
        }

        [HttpPost("RegistrarPagoCliente")]
        public async Task<ActionResult> RegistrarPagoCliente(PagoClienteRequest pago)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, idUsuario) = contexto.Value;
            pago.IdEmpresa = idEmpresa;
            pago.IdUsuario = idUsuario;

            var resultado = await _bancoFlujo.RegistrarPagoCliente(pago);

            if (resultado == 0) return BadRequest("Complete todos los campos obligatorios primero.");
            if (resultado == 2) return BadRequest("El monto del pago debe ser mayor a cero.");
            if (resultado == 4) return BadRequest("La cuenta bancaria seleccionada no existe o no pertenece a la empresa.");
            if (resultado == 5) return BadRequest("La cuenta por cobrar no fue encontrada.");
            if (resultado == 6) return BadRequest("El monto a pagar no puede superar el saldo pendiente de la factura.");
            if (resultado != 1) return BadRequest("Ocurrió un error al registrar el pago.");

            return Ok("Pago de factura registrado con éxito.");
        }

        [HttpGet("ObtenerMovimientos")]
        public async Task<ActionResult<MovimientoBancarioRespuesta>> ObtenerMovimientosBancarios([FromQuery] MovimientoBancarioFiltro filtro)
        {
            var contexto = await ObtenerContextoUsuario();
            if (contexto == null) return Unauthorized("No se pudo identificar al usuario.");

            var (idEmpresa, _) = contexto.Value;

            filtro ??= new MovimientoBancarioFiltro();
            filtro.IdEmpresa = idEmpresa;

            var resultado = await _bancoFlujo.ObtenerMovimientosBancarios(filtro);
            return Ok(resultado);
        }

        #region "Contexto de Usuario"

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
