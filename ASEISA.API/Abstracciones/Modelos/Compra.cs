using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Abstracciones.Modelos;

public class CompraRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int IdProveedor { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdTerminoPago { get; set; }

    [Range(typeof(decimal), "0", "100",
    ParseLimitsInInvariantCulture = true)]
    public decimal PorcentajeDescuento { get; set; }

    public bool ConfirmacionLlegada { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdEstado { get; set; }

    public DateTime? FechaRequerida { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Ingrese al menos un producto.")]
    [MaxLength(500, ErrorMessage = "Se permiten hasta 500 productos.")]
    public List<CompraProductoRequest> Productos { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(PorcentajeDescuento, 2) != PorcentajeDescuento)
            yield return new ValidationResult(
                "El porcentaje de descuento admite hasta dos decimales.",
                new[] { nameof(PorcentajeDescuento) });

        if (Productos != null)
        {
            if (Productos.Any(p => p == null))
                yield return new ValidationResult(
                    "La lista no puede contener productos nulos.", new[] { nameof(Productos) });

            if (Productos.Where(p => p != null).GroupBy(p => p.IdProducto).Any(g => g.Count() > 1))
                yield return new ValidationResult(
                    "No repita un producto; consolide su cantidad en una linea.",
                    new[] { nameof(Productos) });
        }
    }
}

public class CompraProductoRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99",
    ParseLimitsInInvariantCulture = true)]
    public decimal PrecioUnitario { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(PrecioUnitario, 2) != PrecioUnitario)
            yield return new ValidationResult(
                "El precio unitario admite hasta dos decimales.", new[] { nameof(PrecioUnitario) });
    }
}

public class CompraResponse
{
    public int IdCompra { get; set; }
    public int IdEmpresa { get; set; }
    public string? NombreComercial { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string NumeroOrden { get; set; } = string.Empty;
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public int IdUsuarioSolicitante { get; set; }
    public int? IdTerminoPago { get; set; }
    public string? TerminoPago { get; set; }
    public int? DiasCredito { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaRequerida { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? PorcentajeDescuento { get; set; }
    public decimal? MontoDescuento { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
    public bool? ConfirmacionLlegada { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<CompraProductoResponse> Productos { get; set; } = new();
}

public class CompraProductoResponse
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? PorcentajeIVA { get; set; }
    public decimal? MontoDescuento { get; set; }
    public decimal? IVA { get; set; }
    public decimal? Total { get; set; }
}

public class CompraGuardadaResponse
{
    public int IdCompra { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
}

public class CompraFiltro
{
    [StringLength(200)]
    public string? Busqueda { get; set; }

    [Range(1, int.MaxValue)]
    public int? IdEstado { get; set; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int TamanoPagina { get; set; } = 20;
}

public class CompraPaginacionResponse
{
    public long TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public long TotalPaginas { get; set; }
}

public class CompraPaginaResponse
{
    public List<CompraResponse> Compras { get; set; } = new();
    public CompraPaginacionResponse Paginacion { get; set; } = new();
}

public class CompraCatalogosResponse
{
    public List<CompraProveedorCatalogoResponse> Proveedores { get; set; } = new();
    public List<CompraProductoCatalogoResponse> Productos { get; set; } = new();
    public List<CompraTerminoPagoCatalogoResponse> TerminosPago { get; set; } = new();
    public List<CompraEstadoCatalogoResponse> Estados { get; set; } = new();
}

public class CompraProveedorCatalogoResponse
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? IdContacto { get; set; }
    public int? IdTerminoPagoPredeterminado { get; set; }
}

public class CompraProductoCatalogoResponse
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal? CostoReferencia { get; set; }
    public decimal PorcentajeIVA { get; set; }
}

public class CompraTerminoPagoCatalogoResponse
{
    public int IdTerminoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int DiasCredito { get; set; }
}

public class CompraEstadoCatalogoResponse
{
    public int IdEstado { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
