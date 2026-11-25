using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA
{
    public interface IUsuarioDA
    {
        Task ActualizarUltimoAcceso(string correo);

        Task<UsuarioResponse> ObtenerInfoUsuario(string correo);

        Task<int> EditarUsuario(int idEmpresa,int idUsuario, UsuarioEditarRequest usuario, int idUsuarioAccion);

        Task<int> CambiarEstadoUsuario(int idEmpresa,int idUsuario,bool activo,int idUsuarioAccion);

        Task<int> AsignarRolUsuario(int idEmpresa,int idUsuario,int idRol,int idUsuarioAccion);

        Task<int> AprobarRegistroCliente(int idEmpresa,int idUsuario,int idUsuarioAccion,bool aprobar);

        Task<IEnumerable<SolicitudClientePendienteResponse>>ObtenerSolicitudesClientesPendientes(int idEmpresa,int idUsuarioAccion);

        Task<int?> ObtenerEmpresaUsuarioActivo(int idUsuario);

        Task<IEnumerable<UsuarioResponse>> ObtenerUsuarios(int idEmpresa);

        Task<UsuarioResponse?> ObtenerUsuarioPorId(int idEmpresa,int idUsuario);
        Task<int> RetirarRolUsuario(int idEmpresa,int idUsuario,int idRol,int idUsuarioAccion);
        Task<CatalogosUsuarios> ObtenerCatalogos(int idEmpresa,int idUsuarioAccion);
    }
}
