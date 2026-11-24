using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class BalanceGeneralResponse
    {
        public List<BalanceGeneralDetalle> Detalle { get; set; } = new();
        public List<BalanceGeneralTotal> Totales { get; set; } = new();
    }

    public class BalanceGeneralTotal
    {
        [Required]
        public string Clasificacion { get; set; } = string.Empty; 

        public decimal SaldoActual { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal Variacion { get; set; }

        public bool EsProvisional { get; set; }
    }

    public class BalanceGeneralDetalle
    {
        public string CodigoCuenta { get; set; } = string.Empty;
        public string NombreCuenta { get; set; } = string.Empty;

        [Required]
        public string Clasificacion { get; set; } = string.Empty;

        public int Nivel { get; set; }

        public decimal SaldoActual { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal Variacion { get; set; }
    }

    // Filtro

    public class BalanceGeneralFiltro
    {
        [Required(ErrorMessage = "La fecha de corte es obligatoria.")]
        public DateTime FechaCorte1 { get; set; }
        public DateTime? FechaCorte2 { get; set; }
        public bool IncluirSinMovimiento { get; set; } = false;
    }
    

}
