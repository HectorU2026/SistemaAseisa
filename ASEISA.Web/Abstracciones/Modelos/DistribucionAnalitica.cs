using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos;

public class DistribucionAnaliticaRequest : IValidatableObject
{
    public DistribucionAnaliticaRequest(
        int? idProveedor,
        int? idProducto,
        decimal? importe,
        string? descripcion,
        string? referencia,
        string? unidad)
    {
        IdProveedor = idProveedor;
        IdProducto = idProducto;
        Importe = importe;
        Descripcion = descripcion;
        Referencia = referencia;
        Unidad = unidad;
    }

    [Required(ErrorMessage = "Escriba todos los datos requeridos")]
    [Range(1, int.MaxValue)]
    public int? IdProveedor { get; set; }

    [Required(ErrorMessage = "Escriba todos los datos requeridos")]
    [Range(1, int.MaxValue)]
    public int? IdProducto { get; set; }

    [Required(ErrorMessage = "Escriba todos los datos requeridos")]
    public decimal? Importe { get; set; }

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [StringLength(100)]
    public string? Referencia { get; set; }

    [StringLength(50)]
    public string? Unidad { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Importe is decimal importe &&
            (Math.Abs(importe) > 9999999999999999.99m ||
             decimal.Round(importe, 2) != importe))
        {
            yield return new ValidationResult(
                "El importe admite hasta 16 enteros y 2 decimales.",
                new[] { nameof(Importe) });
        }
    }
}

public class DistribucionAnaliticaResponse
{
    public int IdDistribucion { get; set; }
    public int IdProveedor { get; set; }
    public int IdProducto { get; set; }
    public string? Unidad { get; set; }
    public string? Descripcion { get; set; }
    public string? Referencia { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}