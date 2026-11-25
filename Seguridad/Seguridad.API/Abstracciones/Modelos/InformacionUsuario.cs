using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class InformacionUsuarioRequest
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }

    public class InformacionUsuarioResponse
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string IdEmpresa { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
