using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class CuentaPorPagar
    {
        public int IdCuentaPagar { get; set; }
        public string ConsecutivoFactura { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
