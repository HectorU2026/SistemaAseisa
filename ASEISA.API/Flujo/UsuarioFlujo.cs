
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo
{
    public class UsuarioFlujo : IUsuarioFlujo
    {
        private readonly IUsuarioDA _usuarioDA;

        public UsuarioFlujo(IUsuarioDA usuarioDA)
        {
            _usuarioDA = usuarioDA;
        }

        public async Task ActualizarUltimoAcceso(string correo)
        {
            await _usuarioDA.ActualizarUltimoAcceso(correo);
        }

        public async Task<UsuarioResponse> ObtenerInfoUsuario(
            string correo)
        {
            return await _usuarioDA.ObtenerInfoUsuario(correo);
        }

        public async Task<int> EditarUsuario(
            int idEmpresa,
            int idUsuario,
            UsuarioEditarRequest usuario, int idUsuarioAccion)
        {
            return await _usuarioDA.EditarUsuario(
                idEmpresa,
                idUsuario,
                usuario, idUsuarioAccion);
        }

        public async Task<int> CambiarEstadoUsuario(
            int idEmpresa,
            int idUsuario,
            bool activo,
            int idUsuarioAccion)
        {
            return await _usuarioDA.CambiarEstadoUsuario(
                idEmpresa,
                idUsuario,
                activo,
                idUsuarioAccion);
        }

        public async Task<int> AsignarRolUsuario(
            int idEmpresa,
            int idUsuario,
            int idRol,
            int idUsuarioAccion)
        {
            return await _usuarioDA.AsignarRolUsuario(
                idEmpresa,
                idUsuario,
                idRol,
                idUsuarioAccion);
        }

        public async Task<int> AprobarRegistroCliente(
            int idEmpresa,
            int idUsuario,
            int idUsuarioAccion,
            bool aprobar)
        {
            return await _usuarioDA.AprobarRegistroCliente(
                idEmpresa,
                idUsuario,
                idUsuarioAccion,
                aprobar);
        }

        public async Task<IEnumerable<SolicitudClientePendienteResponse>>
            ObtenerSolicitudesClientesPendientes(
                int idEmpresa,
                int idUsuarioAccion)
        {
            return await _usuarioDA.ObtenerSolicitudesClientesPendientes(
                idEmpresa,
                idUsuarioAccion);
        }

        public async Task<int?> ObtenerEmpresaUsuarioActivo(
            int idUsuario)
        {
            if (idUsuario <= 0)
                return null;

            return await _usuarioDA.ObtenerEmpresaUsuarioActivo(
                idUsuario);
        }

        public async Task<IEnumerable<UsuarioResponse>> ObtenerUsuarios(
            int idEmpresa)
        {
            return await _usuarioDA.ObtenerUsuarios(idEmpresa);
        }

        public async Task<UsuarioResponse?> ObtenerUsuarioPorId(
            int idEmpresa,
            int idUsuario)
        {
            return await _usuarioDA.ObtenerUsuarioPorId(
                idEmpresa,
                idUsuario);
        }
        public Task<int> RetirarRolUsuario(int idEmpresa,int idUsuario,int idRol,int idUsuarioAccion)
            =>_usuarioDA.RetirarRolUsuario(idEmpresa,idUsuario,idRol,idUsuarioAccion);
        public Task<CatalogosUsuarios> ObtenerCatalogos(int idEmpresa,int idUsuarioAccion)
            =>_usuarioDA.ObtenerCatalogos(idEmpresa,idUsuarioAccion);    }
}
