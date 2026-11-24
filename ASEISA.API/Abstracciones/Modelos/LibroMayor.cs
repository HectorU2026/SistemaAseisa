using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    using System.ComponentModel.DataAnnotations;

    public class LibroMayorRequest
    {
        [Required]
        public int IdCuentaContable { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }
    }

    public class LibroMayorResponse
    {
        public LibroMayorEncabezado Cuenta { get; set; } = new();
        public List<LibroMayorMovimiento> Movimientos { get; set; } = new();
    }

    public class LibroMayorEncabezado
    {
        public string CodigoCuenta { get; set; } = string.Empty;
        public string NombreCuenta { get; set; } = string.Empty;
        public string Naturaleza { get; set; } = string.Empty;  
        public decimal SaldoInicial { get; set; }
        public decimal TotalDebito { get; set; }
        public decimal TotalCredito { get; set; }
        public decimal SaldoFinal { get; set; }
    }

    public class LibroMayorMovimiento
    {
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Debito { get; set; }
        public decimal Credito { get; set; }
        public decimal SaldoAcumulado { get; set; }
    }
}
