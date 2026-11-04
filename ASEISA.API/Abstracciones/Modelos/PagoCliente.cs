using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class PagoClienteRequest
    {
        public int? IdEmpresa { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdCuentaCobrar { get; set; }
        public int? IdCuentaBancaria { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal? Monto { get; set; }
        public string MetodoPago { get; set; }
        public string Referencia { get; set; }
        public int? IdEstado { get; set; }
    }
}
