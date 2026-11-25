namespace Abstracciones.Modelos
{
    // Contrato interno de revalidacion. No contiene credenciales ni se devuelve por HTTP.
    public class ContextoUsuarioAutorizado
    {
        public long? VersionCredencial { get; set; }
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public List<RolUsuarioAutorizado> Roles { get; set; } = new();
    }

    public class RolUsuarioAutorizado
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}