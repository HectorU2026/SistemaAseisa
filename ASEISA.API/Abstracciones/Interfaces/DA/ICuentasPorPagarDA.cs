using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text; 

namespace Abstracciones.Interfaces.DA
{
    public interface ICuentasPorPagarDA
    {
        Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar();
    }
}
