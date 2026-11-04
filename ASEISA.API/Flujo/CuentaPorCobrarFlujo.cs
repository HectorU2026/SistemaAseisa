using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class CuentaPorCobrarFlujo : ICuentaPorCobrarFlujo
    {
        private readonly ICuentaPorCobrarDA _cuentaPorCobrarDA;
        public CuentaPorCobrarFlujo(ICuentaPorCobrarDA cuentaPorCobrarDA)
        {
            _cuentaPorCobrarDA = cuentaPorCobrarDA;
        }
        public async Task<IEnumerable<CuentaPorCobrarResponse>> ObtenerCuentasPorCobrar(int idEmpresa, int idUsuario)
        {
            if (idEmpresa <= 0 || idUsuario <= 0)
            {
                // Retornamos lista vacía si los identificadores no son válidos
                return Enumerable.Empty<CuentaPorCobrarResponse>();
            }

            return await _cuentaPorCobrarDA.ObtenerCuentasPorCobrar(idEmpresa, idUsuario);
        }
    }
}
