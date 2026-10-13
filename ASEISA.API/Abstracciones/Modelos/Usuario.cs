using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class UsuarioRequest
    {
        [StringLength(50)]
        public string? TipoCliente { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es requerido.")]
        [StringLength(100)]
        public string PrimerApellido { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(500)]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estado valido.")]
        public int IdEstado { get; set; }

        [Phone(ErrorMessage = "Ingrese un telefono valido.")]
        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        public string? Identificacion { get; set; }

        [StringLength(200)]
        public string? NombreRazonSocial { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol valido.")]
        public int IdRol { get; set; }
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
        public string ContrasenaHash { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public int IdEstado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}