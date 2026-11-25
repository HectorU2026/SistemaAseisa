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
        [StringLength(50)]
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
        [StringLength(150)]
        [ASEISA.Usuarios.CorreoUsuario]
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
        [StringLength(30)]
        [ASEISA.Usuarios.TelefonoUsuario]
        public string? Telefono { get; set; }

        [StringLength(250,ErrorMessage="La dirección no puede superar los 250 caracteres.")]
        public string? Direccion { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
            =>ASEISA.Usuarios.ReglasRegistroUsuario.Identificacion(TipoCliente,Identificacion,RazonSocial,nameof(RazonSocial));
        public string GenerarNombreUsuario()=>ASEISA.Usuarios.ReglasRegistroUsuario.Generar(Nombre,PrimerApellido,Identificacion);
    }
}