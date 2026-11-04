using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class CuentaPorCobrarResponse
    {
        public int IdCuentaCobrar { get; set; }
        public string NombreCliente { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoMorosidad { get; set; } // 'Al día' o 'Atrasado'
        public string EstadoRegistro { get; set; }
    }
}