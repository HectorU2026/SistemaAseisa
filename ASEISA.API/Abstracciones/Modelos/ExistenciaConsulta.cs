using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class ExistenciaConsulta
    {
        public int IdProducto { get; set; }
        public int IdBodega { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadReservada { get; set; }
    }
}
