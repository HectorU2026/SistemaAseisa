using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class InventarioFisico
    {
        public int? IdEmpresa { get; set; }
        public int? IdProducto { get; set; }
        public int? IdBodega { get; set; }
        public int? IdUsuario { get; set; }
        public decimal? CantidadDelInventario { get; set; }
        public decimal? ContadoFisicamente { get; set; }
    }
}
