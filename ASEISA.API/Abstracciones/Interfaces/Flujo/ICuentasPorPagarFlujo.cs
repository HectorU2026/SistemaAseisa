using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICuentasPorPagarFlujo
    {
        Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar(int idEmpresa, string? busqueda, int? idEstado, DateTime? fechaVencimiento);
    }
}
