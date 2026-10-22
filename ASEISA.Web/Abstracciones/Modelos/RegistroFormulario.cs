using System.ComponentModel.DataAnnotations;

namespace Web.Modelos
{
    public class RegistroFormulario: IValidatableObject
    {
        [Required(ErrorMessage = "Debe seleccionar un tipo de cliente.")]
        [RegularExpression(@"^(Persona física|Persona jurídica)$",
        ErrorMessage = "Seleccione un tipo de cliente válido.")]
        public string TipoCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(80)]
        public string PrimerApellido { get; set; } = string.Empty;

        [StringLength(80)]
        public string? SegundoApellido { get; set; }

        [StringLength(200)]
        public string? RazonSocial { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe contener al menos una letra mayúscula y un número.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar su contraseña.")]
        [Compare(nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una empresa.")]
        public int? IdEmpresa { get; set; }

        [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
        public string? Telefono { get; set; }

        public string? Direccion { get; set; }
        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (TipoCliente == "Persona jurídica" &&
                string.IsNullOrWhiteSpace(RazonSocial))
            {
                yield return new ValidationResult(
                    "La razón social es obligatoria para personas jurídicas.",
                    new[] { nameof(RazonSocial) }
                );
            }

            string identificacion = Identificacion?.Replace("-", "").Trim() ?? "";

            if (TipoCliente == "Persona física")
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(
                    identificacion, @"^[1-9]\d{8}$"))
                {
                    yield return new ValidationResult(
                        "La cédula física debe contener 9 dígitos y comenzar con un número del 1 al 9.",
                        new[] { nameof(Identificacion) }
                    );
                }
            }
            else if (TipoCliente == "Persona jurídica")
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(
                    identificacion, @"^3\d{9}$"))
                {
                    yield return new ValidationResult(
                        "La cédula jurídica debe contener 10 dígitos y comenzar con 3.",
                        new[] { nameof(Identificacion) }
                    );
                }
            }
        }

        public string GenerarNombreUsuario()
        {
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(PrimerApellido))
                return string.Empty;

            string identificacion = new string(
                (Identificacion ?? "")
                .Where(char.IsDigit)
                .ToArray());

            if (identificacion.Length < 4)
                return string.Empty;

            string inicial = Nombre.Trim()[0].ToString();
            string apellido = PrimerApellido.Trim().Split(' ')[0];

            string usuario = $"{inicial}{apellido}.{identificacion[^4..]}";

            string normalizado = usuario.Normalize(
                System.Text.NormalizationForm.FormD);

            string resultado = new string(
                normalizado
                .Where(c => System.Globalization.CharUnicodeInfo
                    .GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());

            return resultado.ToLowerInvariant();
        }
    }
}

