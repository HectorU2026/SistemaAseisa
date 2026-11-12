using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using DA;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class CuentasPorPagarFlujo : ICuentasPorPagarFlujo   
    {
        private readonly ICuentasPorPagarDA _cuentasPorPagarDA;

        public CuentasPorPagarFlujo(ICuentasPorPagarDA cuentasPorPagarDA)
        {
            _cuentasPorPagarDA = cuentasPorPagarDA;
        }

        public async Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar(int idEmpresa, string? busqueda, int? idEstado, DateTime? fechaVencimiento)
        {
            var cuentas = await _cuentasPorPagarDA.ObtenerCuentasPorPagar(idEmpresa, busqueda, idEstado, fechaVencimiento);

            return(cuentas);
        }
    }
}
