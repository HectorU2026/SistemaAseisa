using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using DA;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Flujo
{
    public class BancoFlujo : IBancoFlujo   
    {
        private IBancoDA _bancoDA;

        public BancoFlujo(IBancoDA bancoDA)
        {
            _bancoDA = bancoDA;
        }

        public async Task<string> RegistrarCuentaBancaria(CuentaBancariaRequest cuentaBancaria)
        {
            return await _bancoDA.RegistrarCuentaBancaria(cuentaBancaria);
        }

        public async Task<ActionResult> ObtenerCuentasBancarias(int idEmpresa)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            var cuentas = await _bancoDA.ObtenerCuentasBancarias(idEmpresa);
            return new OkObjectResult(cuentas);
        }


        public async Task<ActionResult> ObtenerTiposCuenta()
        {
            var tiposCuenta = await _bancoDA.ObtenerTiposCuenta();
            return new OkObjectResult(tiposCuenta);
        }

        public async Task<ActionResult> ObtenerCuentasContables(int idEmpresa)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            var cuentasContables = await _bancoDA.ObtenerCuentasContables(idEmpresa);
            return new OkObjectResult(cuentasContables);
        }

        public async Task<int> RegistrarPagoCliente(PagoClienteRequest pago)
        {
            if (pago == null
                || !pago.IdEmpresa.HasValue || pago.IdEmpresa <= 0
                || !pago.IdUsuario.HasValue || pago.IdUsuario <= 0
                || !pago.IdCuentaCobrar.HasValue || pago.IdCuentaCobrar <= 0
                || !pago.IdCuentaBancaria.HasValue || pago.IdCuentaBancaria <= 0
                || !pago.FechaPago.HasValue
                || !pago.Monto.HasValue
                || string.IsNullOrWhiteSpace(pago.MetodoPago)
                || !pago.IdEstado.HasValue || pago.IdEstado <= 0)
            {
                return 0; // Campos incompletos
            }

            if (pago.Monto <= 0)
            {
                return 2; // El pago debe ser mayor a cero
            }

            return await _bancoDA.RegistrarPagoCliente(pago);
        }

        private static void ValidarId(int valor, string nombre)
        {
            if (valor <= 0)
                throw new ValidationException($"{nombre} debe ser mayor que cero.");
        }
    }
}
