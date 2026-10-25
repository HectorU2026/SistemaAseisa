using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using static Abstracciones.Modelos.Facturacion;

namespace Abstracciones.Interfaces.Flujo
{
    public interface ICotizacionFlujo
    {
        public Task<int> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion);
        Task<CotizacionResponseDetallada?> ObtenerCotizacionPorId(int idCotizacion);
        Task<IActionResult> EnviarCotizacionPorCorreo(int idCotizacion, string correo);
        Task<int> RetrocederCotizacionEnviada(int idCotizacion);
        Task<ActionResult> AvanzarCotizacionAOrdenDeVenta(int idCotizacion);
        Task<ActionResult> Facturar(int idCotizacion, int idUsuario, FacturarRequest request);
        Task<ActionResult> ObtenerCanceladasRechazadas();
        public Task<IEnumerable<CotizacionResponse>> ObtenerCotizacionesFlujo(int idEmpresa);
        public Task<int> EditarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion);
        public Task<int> CancelarCotizacionFlujo(int idEmpresa, int idUsuario, int idCotizacion);
    }
}
