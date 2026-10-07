using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos.Seguridad
{
    public class Login
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Ingrese un correo valido")]
        public string Correo { get; set; }

        [Required]
        [PasswordPropertyText]
        public string ContrasenaHash { get; set; }

    }
    public class restablecerContraseña
    {
        public string Correo { get; set; }
    }

    public class CambiarContrasenaRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
