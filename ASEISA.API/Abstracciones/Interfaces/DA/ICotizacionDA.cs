using Abstracciones.Modelos;
using Abstracciones.Modelos.Servicios;
using System;
using System.Collections.Generic;
using System.Text;
using static Abstracciones.Modelos.Facturacion;

namespace Abstracciones.Interfaces.DA
{
    public interface ICotizacionDA
    {
        public Task<int> RegistrarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> AvanzarCotizacionEnviada(int idEmpresa, int idUsuario, int idCotizacion);
        Task<CotizacionResponse> ObtenerCotizacionPorId(int idCotizacion);
        Task<int> RetrocederCotizacionEnviada(int idCotizacion);
        Task<int> AvanzarCotizacionAOrdenDeVenta(int idCotizacion);
        Task<FacturacionResponse?> FacturarOrdenDeVenta(FacturarCotizacion factura);
        Task<IEnumerable<CotizacionResponse>> ObtenerCanceladasRechazadas();
        Task<IEnumerable<FaltanteInventarioResponse>> ValidarInventarioCotizacion(int idCotizacion);
    }
}
