using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface ICotizacionDA
    {
        public Task<int> RegistrarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
    }
}
