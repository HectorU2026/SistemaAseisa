using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class EmpresaFlujo : IEmpresaFlujo
    {
        private IEmpresaDA _empresaDA;

        public EmpresaFlujo(IEmpresaDA empresaDA)
        {
            _empresaDA = empresaDA;
        }

        public async Task<IEnumerable<EmpresaResponseNombres>> ObtenerNombreEmpresa()
        {
            return await _empresaDA.ObtenerNombreEmpresa();
        }
    }
}
