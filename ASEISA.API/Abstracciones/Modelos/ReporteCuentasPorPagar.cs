using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class ReporteCuentaPorPagarRequest
    {
        public int? IdProveedor { get; set; }
        public bool SoloVencidas { get; set; } = false;
        public DateTime? FechaCorte { get; set; }
    }

    public class ReporteCuentaPorPagarResponse
    {
        public ReporteCuentaPorPagarResumen Resumen { get; set; } = new();
        public List<ReporteCuentaPorPagarProveedor> Proveedores { get; set; } = new();
    }

    public class ReporteCuentaPorPagarResumen
    {
        public DateTime FechaCorte { get; set; }
        public decimal TotalPorPagar { get; set; }
        public decimal TotalVencido { get; set; }
        public decimal TotalPorVencer { get; set; }
        public int CantidadFacturas { get; set; }
        public string? Mensaje { get; set; }
    }

    public class ReporteCuentaPorPagarProveedor
    {
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public decimal TotalAdeudado { get; set; }
        public decimal TotalVencido { get; set; }
        public List<ReporteCuentaPorPagarDetalle> Facturas { get; set; } = new();
    }

    public class ReporteCuentaPorPagarDetalle
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public int IdCuentaPagar { get; set; }
        public string NumeroFacturaCompra { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PagosAplicados { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoVencimiento { get; set; } = string.Empty;
        public int DiasAtraso { get; set; }

        // Calculada para la barra de progreso    
        public decimal PorcentajePagado => MontoTotal > 0
            ? Math.Round((PagosAplicados / MontoTotal) * 100, 2)
            : 0;
    }

}
