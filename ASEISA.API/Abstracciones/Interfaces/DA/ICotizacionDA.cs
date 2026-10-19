using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface ICotizacionDA
    {
        public Task<int> RegistrarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> AvanzarCotizacionEnviada(int idEmpresa, int idUsuario, int idCotizacion);
        public Task<IEnumerable<CotizacionResponse>> ObtenerCotizaciones(int idEmpresa);
        public Task<int> EditarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
    }
}
