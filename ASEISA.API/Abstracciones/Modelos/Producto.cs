using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Producto
    {
        public int? IdProducto { get; set; }
        public int? IdEmpresa { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string TipoProducto { get; set; }
        public decimal? PrecioVenta { get; set; }
        public decimal? PorcentajeImpuestoVenta { get; set; }
        public decimal? CostoPromedio { get; set; }
        public decimal? PorcentajeImpuestoCompra { get; set; }
        public string PoliticaFacturacion { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Volumen { get; set; }
        public int? TiempoEntrega { get; set; }
        public int? IdCuentaIngreso { get; set; }
        public int? IdCuentaGasto { get; set; }
        public int? IdBodega { get; set; }
        public decimal? Cantidad { get; set; }
        public int? IdEstado { get; set; }
    }
}
