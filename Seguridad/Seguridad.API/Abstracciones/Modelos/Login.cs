using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class LoginBase
    {
        [Required]
        public string ContrasenaHash { get; set; }
        [Required]
        [EmailAddress]
        public string Correo { get; set; }
    }

    public class LoginAutenticado : LoginBase
    {
        public int id { get; set; }

        public int IdUsuario { get => id; set => id = value; }
        public string NombreUsuario { get; set; }
        public string rol { get; set; }
    }
}
