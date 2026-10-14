using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICotizacionFlujo
    {
        public Task<int> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
    }
}
