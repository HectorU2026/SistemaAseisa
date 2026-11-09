using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IEmpresaDA
    {
        Task<IEnumerable<EmpresaResponseNombres>> ObtenerNombreEmpresa();
        Task<Empresa?> ObtenerEmpresaPorId(int idEmpresa);
    }
}
