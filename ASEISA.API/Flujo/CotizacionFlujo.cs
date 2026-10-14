using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class CotizacionFlujo : ICotizacionFlujo
    {
        private ICotizacionDA _cotizacionDA;

        public CotizacionFlujo(ICotizacionDA cotizacionDA)
        {
            _cotizacionDA = cotizacionDA;
        }

        public async Task<int> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            return await _cotizacionDA.RegistrarCotizacion(idEmpresa, idUsuario, cotizacion);
        }

        // avanzar cotizacion enviada
        public async Task<int> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            return await _cotizacionDA.AvanzarCotizacionEnviada(idEmpresa, idUsuario, idCotizacion);
        }
    }
}
