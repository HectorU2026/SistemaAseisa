using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Bodega
    {
        public int? IdBodega { get; set; }
        public int IdEmpresa { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Ubicacion { get; set; }
        public int IdEstado { get; set; }
    }
}
