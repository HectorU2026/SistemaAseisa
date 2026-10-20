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
        public string? NombreEstado { get; set; }
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


    //////////// CotizacionResponse
    ///

    public class CotizacionResponse
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
        public string? NombreEstado { get; set; }
        public bool EstadoValido { get; set; }

        public EmpresaCotizacion Empresa { get; set; } = new();
        public ClienteCotizacion Cliente { get; set; } = new();
        public List<DetalleCotizacion> Detalles { get; set; } = new();

        public class EmpresaCotizacion
        {
            public string? NombreComercial { get; set; }
            public string? RazonSocial { get; set; }
            public string? CedulaJuridica { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? Direccion { get; set; }
        }

        public class ClienteCotizacion
        {
            public string? NombreRazonSocial { get; set; }
            public string? Identificacion { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? Direccion { get; set; }
        }

        public class DetalleCotizacion
        {
            public int IdDetalleCotizacion { get; set; }
            public int IdCotizacion { get; set; }
            public int IdProducto { get; set; }
            public string? CodigoProducto { get; set; }
            public string? NombreProducto { get; set; }
            public string? DescripcionProducto { get; set; }
            public string? UnidadMedida { get; set; }
            public decimal Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal? PorcentajeDescuento { get; set; }
            public decimal? PorcentajeImpuesto { get; set; }
            public decimal Subtotal { get; set; }
        }
    }


}
