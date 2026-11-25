using System.ComponentModel.DataAnnotations;
namespace Abstracciones.Modelos
{
    public class RegistroAdministrativoRequest : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            foreach(var e in ASEISA.Usuarios.ReglasRegistroUsuario.Identificacion(TipoCliente,Identificacion,NombreRazonSocial,nameof(NombreRazonSocial)))yield return e;
            foreach(var e in ASEISA.Usuarios.ReglasRegistroUsuario.Roles(IdsRoles,IdRol))yield return e;
        }
        public List<int> IdsRoles {get;set;}=new();
        [Required(ErrorMessage="Este campo es obligatorio."), StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")] public string Nombre { get; set; } = "";
        [Required(ErrorMessage="Este campo es obligatorio."), StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")] public string PrimerApellido { get; set; } = "";
        [StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")] public string? SegundoApellido { get; set; }
        [Required(ErrorMessage="El correo es obligatorio."), EmailAddress(ErrorMessage="Ingrese un correo electrónico válido."), StringLength(150),ASEISA.Usuarios.CorreoUsuario] public string Correo { get; set; } = "";
        [StringLength(100)] public string NombreUsuario { get; set; } = "";
         public int IdRol { get; set; }
        [Range(1,int.MaxValue,ErrorMessage="Seleccione un estado válido.")] public int IdEstado { get; set; }
        [StringLength(50)] public string? TipoCliente { get; set; }
        [Phone(ErrorMessage="Ingrese un teléfono válido."),StringLength(30),ASEISA.Usuarios.TelefonoUsuario] public string? Telefono { get; set; }
        [StringLength(250,ErrorMessage="La dirección no puede superar los 250 caracteres.")] public string? Direccion { get; set; }
        [StringLength(50)] public string? Identificacion { get; set; }
        [StringLength(200)] public string? NombreRazonSocial { get; set; }
    }
    public class ResultadoInvitacion
    {
        public int IdUsuario { get; set; }
        public bool EnvioProgramado { get; set; }
        public bool EnlaceEnviado { get; set; }
    }
    public class DestinatarioContrasena
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
    }
}