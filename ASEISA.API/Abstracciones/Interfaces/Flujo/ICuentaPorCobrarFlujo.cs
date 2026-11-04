using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICuentaPorCobrarFlujo
    {
        Task<IEnumerable<CuentaPorCobrarResponse>> ObtenerCuentasPorCobrar(int idEmpresa, int idUsuario);
    }
}
