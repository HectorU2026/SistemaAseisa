using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class ActivoFijoRequest
    {
        public int? IdEmpresa { get; set; }
        public int? IdUsuario { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del activo es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de adquisición es obligatoria.")]
        public DateTime? FechaAdquisicion { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El costo de adquisición es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a cero.")]
        public decimal? CostoAdquisicion { get; set; }

        [Required(ErrorMessage = "El valor residual es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor residual no puede ser negativo.")]
        public decimal? ValorResidual { get; set; }

        [Required(ErrorMessage = "La vida útil en meses es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La vida útil debe ser al menos 1 mes.")]
        public int? VidaUtilMeses { get; set; }

        [Required(ErrorMessage = "El método de depreciación es obligatorio.")]
        public string MetodoDepreciacion { get; set; } = "Línea Recta";

        [Required(ErrorMessage = "Debe seleccionar la cuenta contable del activo.")]
        public int? IdCuentaActivo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la cuenta contable de depreciación.")]
        public int? IdCuentaDepreciacion { get; set; }

        public int? IdEstado { get; set; }
    }
}