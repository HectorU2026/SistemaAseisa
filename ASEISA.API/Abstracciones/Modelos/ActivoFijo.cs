using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class ActivoFijoRequest
    {
        public int? IdEmpresa { get; set; }
        public int? IdUsuario { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaAdquisicion { get; set; }
        public decimal? CostoAdquisicion { get; set; }
        public decimal? ValorResidual { get; set; }
        public int? VidaUtilMeses { get; set; }
        public string MetodoDepreciacion { get; set; }
        public int? IdCuentaActivo { get; set; }
        public int? IdCuentaDepreciacion { get; set; }
        public int? IdEstado { get; set; }
    }
}