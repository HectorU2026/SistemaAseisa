using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICuentasPorPagarFlujo
    {
        Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar();
    }
}
