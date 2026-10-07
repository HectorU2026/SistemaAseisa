using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos;

public static class RolesContacto
{
    public const string Cliente = "Cliente";
    public const string Proveedor = "Proveedor";
    public const string Ambos = "Ambos";
}

public class ContactoRequest : IValidatableObject
{
    [StringLength(50)]
    public string? Cedula { get; set; }

    [StringLength(50)]
    public string? IdentificacionFiscal { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es requerido.")]
    [Phone(ErrorMessage = "Ingrese un telefono valido.")]
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El pais es requerido.")]
    [StringLength(100)]
    public string Pais { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Provincia { get; set; }

    [StringLength(100)]
    public string? Canton { get; set; }

    [StringLength(100)]
    public string? Distrito { get; set; }

    [Required(ErrorMessage = "El detalle de direccion es requerido.")]
    [StringLength(300)]
    public string DetalleDireccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol del contacto es requerido.")]
    [RegularExpression("^(Cliente|Proveedor|Ambos)$", ErrorMessage = "Seleccione Cliente, Proveedor o Ambos.")]
    public string RolContacto { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estado valido.")]
    public int IdEstado { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdVendedor { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdComprador { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdTerminoPagoVentas { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdMetodoPagoVentas { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdTerminoPagoCompras { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdMetodoPagoCompras { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Cedula) && string.IsNullOrWhiteSpace(IdentificacionFiscal))
            yield return new ValidationResult("Ingrese cedula o identificacion fiscal.",
                new[] { nameof(Cedula), nameof(IdentificacionFiscal) });

        if (string.Equals((Pais ?? string.Empty).Trim(), "Costa Rica", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(Cedula))
                yield return new ValidationResult("La cedula es requerida para Costa Rica.", new[] { nameof(Cedula) });
            if (string.IsNullOrWhiteSpace(Provincia))
                yield return new ValidationResult("La provincia es requerida para Costa Rica.", new[] { nameof(Provincia) });
            if (string.IsNullOrWhiteSpace(Canton))
                yield return new ValidationResult("El canton es requerido para Costa Rica.", new[] { nameof(Canton) });
            if (string.IsNullOrWhiteSpace(Distrito))
                yield return new ValidationResult("El distrito es requerido para Costa Rica.", new[] { nameof(Distrito) });
        }
        else if (!string.IsNullOrWhiteSpace(Pais) && string.IsNullOrWhiteSpace(IdentificacionFiscal))
        {
            yield return new ValidationResult("El contacto extranjero requiere identificacion fiscal.",
                new[] { nameof(IdentificacionFiscal) });
        }

        if (RolContacto == RolesContacto.Cliente &&
            (IdComprador.HasValue || IdTerminoPagoCompras.HasValue || IdMetodoPagoCompras.HasValue))
            yield return new ValidationResult("Un cliente no admite configuracion de compras.",
                new[] { nameof(IdComprador), nameof(IdTerminoPagoCompras), nameof(IdMetodoPagoCompras) });

        if (RolContacto == RolesContacto.Proveedor &&
            (IdVendedor.HasValue || IdTerminoPagoVentas.HasValue || IdMetodoPagoVentas.HasValue))
            yield return new ValidationResult("Un proveedor no admite configuracion de ventas.",
                new[] { nameof(IdVendedor), nameof(IdTerminoPagoVentas), nameof(IdMetodoPagoVentas) });
    }
}

public class ContactoResponse
{
    public int IdContacto { get; set; }
    public int IdEmpresa { get; set; }
    public string? Cedula { get; set; }
    public string? IdentificacionFiscal { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string? Provincia { get; set; }
    public string? Canton { get; set; }
    public string? Distrito { get; set; }
    public string DetalleDireccion { get; set; } = string.Empty;
    public string RolContacto { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
    public int? IdProveedor { get; set; }
    public int? IdVendedor { get; set; }
    public int? IdComprador { get; set; }
    public int? IdTerminoPagoVentas { get; set; }
    public int? IdMetodoPagoVentas { get; set; }
    public int? IdTerminoPagoCompras { get; set; }
    public int? IdMetodoPagoCompras { get; set; }
    public string? TerminoPagoVentas { get; set; }
    public string? MetodoPagoVentas { get; set; }
    public string? TerminoPagoCompras { get; set; }
    public string? MetodoPagoCompras { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class ContactoCreadoResponse
{
    public int IdContacto { get; set; }
    public int? IdCliente { get; set; }
    public int? IdProveedor { get; set; }
}

public class ContactoFiltro
{
    [StringLength(200)]
    public string? Busqueda { get; set; }

    [RegularExpression("^(Cliente|Proveedor|Ambos)$")]
    public string? RolContacto { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdEstado { get; set; }
}
