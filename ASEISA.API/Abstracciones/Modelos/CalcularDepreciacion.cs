using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class CalcularDepreciacionRequest
    {
        public int? IdEmpresa { get; set; }
        public int? IdActivoFijo { get; set; }
        public string Periodo { get; set; }
        public int? IdEstado { get; set; }
        public int? IdUsuario { get; set; }
    }
}