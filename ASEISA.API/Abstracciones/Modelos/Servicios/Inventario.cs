using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos.Servicios
{
    public class Inventario
    {
    }

    public class FaltanteInventarioResponse
    {
        public string Producto { get; set; }
        public decimal CantidadRequerida { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal Faltante { get; set; }
    }
}
