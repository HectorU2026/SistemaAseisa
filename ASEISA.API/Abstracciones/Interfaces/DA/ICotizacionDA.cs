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
        Task<CotizacionResponseDetallada?> ObtenerCotizacionPorId(int idCotizacion);
        Task<int> RetrocederCotizacionEnviada(int idCotizacion);
        Task<int> AvanzarCotizacionAOrdenDeVenta(int idCotizacion);
        Task<FacturacionResponse?> FacturarOrdenDeVenta(FacturarCotizacion factura);
        Task<IEnumerable<CotizacionResponseDetallada>> ObtenerCanceladasRechazadas();
        Task<IEnumerable<FaltanteInventarioResponse>> ValidarInventarioCotizacion(int idCotizacion);
        public Task<IEnumerable<CotizacionResponse>> ObtenerCotizaciones(int idEmpresa);
        public Task<int> EditarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> CancelarCotizacion(int idEmpresa, int idUsuario, int idCotizacion);
    }
}
