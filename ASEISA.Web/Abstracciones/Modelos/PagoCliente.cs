using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

public class PagoClienteRequest
{
    public int? IdEmpresa { get; set; }
    public int? IdUsuario { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una factura pendiente.")]
    public int? IdCuentaCobrar { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una cuenta bancaria.")]
    public int? IdCuentaBancaria { get; set; }

    [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
    public DateTime? FechaPago { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto del pago debe ser mayor a cero.")]
    public decimal? Monto { get; set; }

    [Required(ErrorMessage = "El método de pago es obligatorio.")]
    public string MetodoPago { get; set; } = string.Empty;

    public string? Referencia { get; set; }
    public int? IdEstado { get; set; }
}
    