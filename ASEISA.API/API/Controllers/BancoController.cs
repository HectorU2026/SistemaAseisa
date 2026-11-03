using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
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
    }
}
