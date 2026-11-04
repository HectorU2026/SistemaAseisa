using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class MovimientoBancarioFiltro
    {
        public int? IdEmpresa { get; set; }
        public int? IdCuentaBancaria { get; set; }
        public int? Pagina { get; set; }
        public int? TamanoPagina { get; set; }
    }

    public class MovimientoBancarioItem
    {
        public int IdMovimientoBancario { get; set; }
        public string NumeroCuenta { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public string Referencia { get; set; }
        public string Estado { get; set; }
    }

    public class MovimientoBancarioRespuesta
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int Total { get; set; }
        public IEnumerable<MovimientoBancarioItem> Items { get; set; }
    }
}
