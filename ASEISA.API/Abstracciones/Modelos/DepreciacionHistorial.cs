using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class DepreciacionHistorialResponse
    {
        public int IdDepreciacion { get; set; }
        public int IdActivoFijo { get; set; }
        public string NombreActivo { get; set; }
        public decimal CostoAdquisicion { get; set; }
        public string Periodo { get; set; }
        public decimal MontoDespreciacion { get; set; } 
        public decimal DepreciacionAcumulada { get; set; }
        public decimal ValorEnLibros { get; set; }
        public DateTime FechaCalculo { get; set; }
        public string Estado { get; set; }
    }
}
