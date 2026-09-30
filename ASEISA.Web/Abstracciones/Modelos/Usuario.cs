using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Usuario
    {
        public int? IdUsuario { get; set; }
        public int? IdEmpresa { get; set; }
        public string? TipoCliente { get; set; }
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El campo Primer apellido es requerido")]
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        [Required(ErrorMessage = "El campo Correo es requerido")]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Ingrese un correo valido")]
        public string Correo { get; set; }
        [Required(ErrorMessage = "El campo Nombre de usuario es requerido")]
        public string NombreUsuario { get; set; }
        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        public string ContrasenaHash { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int? IdEstado { get; set; }
    }

}
