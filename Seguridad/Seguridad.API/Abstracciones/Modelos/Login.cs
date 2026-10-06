using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string NombreUsuario { get; set; }
        public string rol { get; set; }
    }
}
    