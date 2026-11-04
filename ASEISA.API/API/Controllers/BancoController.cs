using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Flujo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BancoController : Controller
    {
        private IBancoFlujo _bancoFlujo;

        public BancoController(IBancoFlujo bancoFlujo)
        {
            _bancoFlujo = bancoFlujo;
        }
        [HttpPost("RegistrarCuentaBancaria")]
        public async Task<ActionResult> RegistrarCuentaBancaria(CuentaBancariaRequest cuentaBancaria)
        {
            await _bancoFlujo.RegistrarCuentaBancaria(cuentaBancaria);
            return Ok();
        }

        [HttpGet("ObtenerCuentasBancarias")]
        public async Task<ActionResult> ObtenerCuentasBancarias(int idEmpresa)
        {
            return await _bancoFlujo.ObtenerCuentasBancarias(idEmpresa);
        }

        [HttpGet("ObtenerTiposCuenta")]
        public async Task<ActionResult> ObtenerTiposCuenta()
        {
            return await _bancoFlujo.ObtenerTiposCuenta();
        }

        [HttpGet("ObtenerCuentasContables")]
        public async Task<ActionResult> ObtenerCuentasContables(int idEmpresa)
        {
            return await _bancoFlujo.ObtenerCuentasContables(idEmpresa);
        }

        [HttpPost("RegistrarPagoCliente")]
        public async Task<ActionResult> RegistrarPagoCliente(PagoClienteRequest pago)
        {
            var resultado = await _bancoFlujo.RegistrarPagoCliente(pago);

            if (resultado == 0) return BadRequest("Complete todos los campos obligatorios primero.");
            if (resultado == 2) return BadRequest("El monto del pago debe ser mayor a cero.");
            if (resultado == 4) return BadRequest("La cuenta bancaria seleccionada no existe o no pertenece a la empresa.");
            if (resultado == 5) return BadRequest("La cuenta por cobrar no fue encontrada.");
            if (resultado == 6) return BadRequest("El monto a pagar no puede superar el saldo pendiente de la factura.");
            if (resultado != 1) return BadRequest("Ocurrió un error al registrar el pago.");

            return Ok("Pago de factura registrado con éxito.");
        }
    }
}
