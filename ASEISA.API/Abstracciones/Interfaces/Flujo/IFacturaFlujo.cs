using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IFacturaFlujo
    {
        Task<ActionResult> ObtenerFacturas(int idUsuario, FacturaFiltro? filtro);
        Task<ActionResult> ObtenerFlujoFacturas(int idUsuario, FlujoFacturaFiltro? filtro);
        Task<ActionResult> ObtenerFlujoFactura(int idUsuario, int idFactura);
        Task<ActionResult> ObtenerReporteFacturacion(int idUsuario, ReporteFacturacionFiltro? filtro);
        Task<ActionResult> ObtenerDetalleReporteFacturacion(int idUsuario, int idFactura);
        Task<ActionResult> ObtenerFacturasPagadas(int idUsuario, FacturaPagadaFiltro? filtro);
        Task<ActionResult> ObtenerFacturaPagada(int idUsuario, int idFactura);
        Task<ActionResult> ObtenerFactura(int idUsuario, int idFactura);
        Task<ActionResult> Confirmar(int idUsuario, int idFactura, byte[]? versionFila);
        Task<ActionResult> Restablecer(int idUsuario, int idFactura, byte[]? versionFila);
        Task<ActionResult> Inactivar(int idUsuario, int idFactura, byte[]? versionFila, string? motivoAnulacion);
        Task<ActionResult> Reactivar(int idUsuario, int idFactura, byte[]? versionFila);
        Task<ActionResult> RegistrarPago(int idUsuario, int idFactura, decimal? monto, string? metodoPago);
        Task<ActionResult> RegistrarNotaCredito(int idUsuario, int idFactura, string? motivo, decimal? monto, string? descripcionAdicional);
        Task<ActionResult> Enviar(int idUsuario, int idFactura);
        Task<ActionResult> DescargarXml(int idUsuario, int idFactura);
        Task<ActionResult> EnviarXml(int idUsuario, int idFactura);
        Task<ActionResult> DescargarPdf(int idUsuario, int idFactura);
    }
}
