
using System;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class MetodoPagoRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class MetodoPagoResponse
    {
        public int IdMetodoPago { get; set; }

        public int IdEmpresa { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }

    public class MetodoPagoCreadoResponse
    {
        public int IdMetodoPago { get; set; }
    }
}
