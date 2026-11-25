using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{

    public class UsuarioRequest : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            foreach(var e in ASEISA.Usuarios.ReglasRegistroUsuario.Identificacion(TipoCliente,Identificacion,NombreRazonSocial,nameof(NombreRazonSocial)))yield return e;
            foreach(var e in ASEISA.Usuarios.ReglasRegistroUsuario.Roles(IdsRoles,IdRol))yield return e;
        }
        public List<int> IdsRoles {get;set;}=new();
        [StringLength(50)]
        public string? TipoCliente { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es requerido.")]
        [StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")]
        public string PrimerApellido { get; set; } = string.Empty;

        [StringLength(80,ErrorMessage="El campo no puede superar los 80 caracteres.")]
        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        [ASEISA.Usuarios.CorreoUsuario]
        public string Correo { get; set; } = string.Empty;


        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public string Contrasena { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estado válido.")]
        public int IdEstado { get; set; }

        [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
        [StringLength(30)]
        [ASEISA.Usuarios.TelefonoUsuario]
        public string? Telefono { get; set; }

        [StringLength(250,ErrorMessage="La dirección no puede superar los 250 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(50)]
        public string? Identificacion { get; set; }

        [StringLength(200)]
        public string? NombreRazonSocial { get; set; }


        public int IdRol { get; set; }
    }

    public class UsuarioRolResponse
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    public class UsuarioResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string? TipoCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public int IdEstado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? IdRol { get; set; }
        public string? NombreRol { get; set; }
        public List<UsuarioRolResponse> Roles { get; set; } = new();
        public string EstadoAprobacion { get; set; } = string.Empty;
    }

    public class UsuarioEditarRequest
    {
        [Required]
        [StringLength(80)]
        public string Nombre { get; set; } = "";

        [Required]
        [StringLength(80)]
        public string PrimerApellido { get; set; } = "";

        [StringLength(80)]
        public string? SegundoApellido { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [ASEISA.Usuarios.CorreoUsuario]
        public string Correo { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = "";
    }

    public class UsuarioCambiarEstadoRequest
    {
        [Required]
        public bool? Activo { get; set; }
    }

    public class UsuarioAsignarRolRequest
    {
        [Range(1, int.MaxValue)]
        public int IdRol { get; set; }
    }

    public class UsuarioAprobarSolicitudRequest
    {
        [Required]
        public bool? Aprobar { get; set; }
    }

    public class SolicitudClientePendienteResponse
    {
        public int IdEstado { get; set; }
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }


        public string Correo { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string? TipoCliente { get; set; }

        public string EstadoAprobacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        public int? IdCliente { get; set; }
        public string? NombreRazonSocial { get; set; }
        public string? Identificacion { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }

}