using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos;
public class CuentaContableResponse
{
    public int IdCuentaContable { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}

public abstract class CuentaContableDatos : IValidatableObject
{
    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public bool? ConciliacionPagos { get; set; }

    public decimal? Impuestos { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Impuestos is decimal valor &&
            (valor < -99999.9999m ||
             valor > 99999.9999m ||
             decimal.Round(valor, 4) != valor))
        {
            yield return new ValidationResult(
                "Impuestos admite hasta cinco dígitos enteros y cuatro decimales.",
                new[] { nameof(Impuestos) });
        }
    }
}

public class EditarCuentaContableRequest : CuentaContableDatos
{
    [Range(1, int.MaxValue)]
    public virtual int? IdCuentaPrincipal { get; set; }
}

public class AgregarCuentaContableRequest : EditarCuentaContableRequest
{
    [Required, Range(1, int.MaxValue)]
    public override int? IdCuentaPrincipal { get; set; }

    [Required, Range(1, 2)]
    public int? IdEstado { get; set; }

    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;
}

public class AgregarCuentaRaizRequest : CuentaContableDatos
{
    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, RegularExpression("Activo|Pasivo|Patrimonio|Ingreso|Gasto")]
    public string TipoCuenta { get; set; } = string.Empty;

    [Required, RegularExpression("Deudora|Acreedora")]
    public string Naturaleza { get; set; } = string.Empty;

    [Required, Range(1, 2)]
    public int? IdEstado { get; set; }
}

public class CuentaContableDetalleResponse
{
    public int IdCuentaContable { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public bool PermiteMovimientos { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? IdCuentaPrincipal { get; set; }
    public string? CuentaPrincipal { get; set; }
    public bool ConciliacionPagos { get; set; }
    public decimal? Impuestos { get; set; }
    public string? Descripcion { get; set; }
}