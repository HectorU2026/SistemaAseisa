using Abstracciones.Modelos;

namespace Web.Models
{
    public class RegistroRequest
    {
        public Usuario Usuario { get; set; } = default!;
        public Cliente Cliente { get; set; } = default!;
    }
}
