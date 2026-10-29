using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
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

        public async Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar()
        {
            return await _cuentasPorPagarDA.ObtenerCuentasPorPagar();
        }
    }
}
