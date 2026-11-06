using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface ICuentaPorCobrarDA
    {
        Task<IEnumerable<CuentaPorCobrarResponse>> ObtenerCuentasPorCobrar(int idEmpresa, int idUsuario);
    }
}
