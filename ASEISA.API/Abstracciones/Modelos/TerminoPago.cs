
using System;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class TerminoPagoRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los días de crédito son obligatorios.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "Los días de crédito no pueden ser negativos.")]
        public int? DiasCredito { get; set; }
    }

    public class TerminoPagoResponse
    {
        public int IdTerminoPago { get; set; }

        public int IdEmpresa { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int DiasCredito { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }

    public class TerminoPagoCreadoResponse
    {
        public int IdTerminoPago { get; set; }
    }
}
