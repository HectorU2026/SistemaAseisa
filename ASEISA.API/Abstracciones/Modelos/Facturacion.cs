using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Facturacion
    {
        public class FacturarRequest
        {
            [Required(ErrorMessage = "El tipo de factura es obligatorio")]
            public string TipoFactura { get; set; } 

            [Required(ErrorMessage = "La condición de venta es obligatoria")]
            public string CondicionVenta { get; set; } = "Contado";

            [Required(ErrorMessage = "La fecha de factura es obligatoria")]
            public DateTime? FechaFactura { get; set; }
        }

        public class FacturacionResponse
        {
            public int IdFactura { get; set; }
            public string? NumeroFactura { get; set; }
            public string Mensaje { get; set; }
        }


        public class FacturarCotizacion
        {
            public int IdCotizacion { get; set; }
            public int IdEmpresa { get; set; }
            public int IdCliente { get; set; }
            public int IdUsuario { get; set; }
            public string NumeroVenta { get; set; } = string.Empty;
            public string NumeroFactura { get; set; } = string.Empty;
            public string TipoFactura { get; set; } = string.Empty;
            public string CondicionVenta { get; set; } = string.Empty;
            public string? Moneda { get; set; }
            public decimal? TipoCambio { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Impuesto { get; set; }
            public decimal Descuento { get; set; }
            public decimal Total { get; set; }
            public DateTime FechaFactura { get; set; }
        }

    }
}
