using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class ReporteFacturacionFiltro
    {
        [StringLength(20)]
        public string? Periodo { get; set; }

        [StringLength(50)]
        public string? TipoFactura { get; set; }

        [StringLength(50)]
        public string? OrdenarPor { get; set; }
    }

    public class ReporteFacturacionLinea
    {
        public string Periodo { get; set; } = string.Empty;
        public string? TipoFactura { get; set; }
        public string? Moneda { get; set; }
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal TotalNotasCreditoFactura { get; set; }
    }

    public class ReporteFacturacionGrupo
    {
        public string Periodo { get; set; } = string.Empty;
        public string? TipoFactura { get; set; }
        public string? Moneda { get; set; }
        public int CantidadFacturas { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalNotasCredito { get; set; }
        public List<ReporteFacturacionLinea> Facturas { get; set; } = new();
    }

    public class ReporteFacturacionResultado
    {
        public IReadOnlyList<ReporteFacturacionGrupo> Grupos { get; set; } = Array.Empty<ReporteFacturacionGrupo>();
    }

    public class ReporteFacturacionConsulta
    {
        public string? Mensaje { get; set; }
        public IReadOnlyList<ReporteFacturacionGrupo> Grupos { get; set; } = Array.Empty<ReporteFacturacionGrupo>();
    }

    public class ReporteNotaCredito
    {
        public int IdNotaCredito { get; set; }
        public string NumeroNotaCredito { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? DescripcionAdicional { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int IdFactura { get; set; }
    }

    public class ReporteFacturacionDetalle
    {
        public required FacturaDetalle Factura { get; set; }
        public IReadOnlyList<ReporteNotaCredito> NotasCredito { get; set; } = Array.Empty<ReporteNotaCredito>();
    }
}
