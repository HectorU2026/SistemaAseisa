using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class CotizacionRequest
    {
        public int IdCotizacion { get; set; }
        public int IdEmpresa { get; set; }
        public int IdCliente { get; set; }
        public string NumeroCotizacion { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? Moneda { get; set; }
        public decimal? TipoCambio { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public int IdEstado { get; set; }
        public List<DetalleCotizacionRequest> Detalles { get; set; } = new();

    }
    public class DetalleCotizacionRequest
    {
        public int IdDetalleCotizacion { get; set; }
        public int IdCotizacion { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? PorcentajeDescuento { get; set; }
        public decimal? PorcentajeImpuesto { get; set; }
        public decimal Subtotal { get; set; }
    }
}
