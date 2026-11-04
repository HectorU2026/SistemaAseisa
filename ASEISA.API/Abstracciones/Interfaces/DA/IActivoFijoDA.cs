using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IActivoFijoDA
    {
        Task<int> RegistrarActivoFijo(ActivoFijoRequest activo);
        Task<int> CalcularDepreciacionActivo(CalcularDepreciacionRequest request);
        Task<IEnumerable<DepreciacionHistorialResponse>> ObtenerHistorialDepreciacion(int idEmpresa, int idActivoFijo);
    }
}
