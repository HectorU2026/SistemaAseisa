using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class FlujoFacturaFiltro
    {
        [Range(1, int.MaxValue)]
        public int? IdCotizacion { get; set; }

        [StringLength(50)]
        public string? NumeroFactura { get; set; }

        [StringLength(50)]
        public string? OrdenarPor { get; set; }
    }

    public class FlujoFacturaListado
    {
        public int IdCotizacion { get; set; }
        public string NumeroCotizacion { get; set; }
        public string EstadoCotizacion { get; set; }
        public decimal TotalOrden { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalCobradoOrden { get; set; }
        public decimal SaldoPendienteOrden { get; set; }
        public int IdVenta { get; set; }
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; }
        public string EstadoFactura { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal TotalFactura { get; set; }
        public decimal SaldoPendienteFactura { get; set; }
        public int CantidadFacturasVenta { get; set; }
        public decimal MontoCobradoVenta { get; set; }
        public decimal? MontoCobradoFactura { get; set; }
    }

    public class FlujoFacturaConsulta
    {
        public string? Mensaje { get; set; }
        public IReadOnlyList<FlujoFacturaListado> Items { get; set; } = Array.Empty<FlujoFacturaListado>();
    }

    public class FlujoFacturaDetalle
    {
        public required FacturaDetalle Factura { get; set; }
        public required FlujoFacturaListado Resumen { get; set; }
    }
}
