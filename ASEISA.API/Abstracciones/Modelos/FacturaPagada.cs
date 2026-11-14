using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class FacturaPagadaFiltro
    {
        [StringLength(200)]
        public string? NombreCliente { get; set; }

        public DateTime? FechaPagoDesde { get; set; }

        public DateTime? FechaPagoHasta { get; set; }
    }

    public class FacturaPagadaListado
    {
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public DateTime? FechaPago { get; set; }
    }

    public class FacturaPagadaConsulta
    {
        public string? Mensaje { get; set; }
        public IReadOnlyList<FacturaPagadaListado> Facturas { get; set; } = Array.Empty<FacturaPagadaListado>();
    }

    public class FacturaPagadaPago
    {
        public int IdPagoCliente { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class FacturaPagadaDetalle
    {
        public required FacturaDetalle Factura { get; set; }
        public IReadOnlyList<FacturaPagadaPago> Pagos { get; set; } = Array.Empty<FacturaPagadaPago>();
    }
}
