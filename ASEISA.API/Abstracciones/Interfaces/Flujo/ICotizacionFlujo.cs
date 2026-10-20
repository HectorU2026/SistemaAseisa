using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICotizacionFlujo
    {
        public Task<int> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion);
        public Task<IEnumerable<CotizacionResponse>> ObtenerCotizacionesFlujo(int idEmpresa);
        public Task<int> EditarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> CancelarCotizacionFlujo(int idEmpresa, int idUsuario, int idCotizacion);
    }
}
