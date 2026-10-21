using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class IngresoProducto
    {
        public int IdEmpresa { get; set; }
        public int IdProducto { get; set; }
        public int IdBodega { get; set; }
        public int IdUsuario { get; set; }
        public int IdContacto { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public decimal? Cantidad { get; set; }
        public string? CotizacionCompra { get; set; }
        public int? IdCotizacion { get; set; }
        public int? IdDistribucionAnalitica { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
