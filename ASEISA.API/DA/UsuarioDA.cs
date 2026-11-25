
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DA
{
    public class UsuarioDA : IUsuarioDA
    {
        private readonly IRepositorioDapper _repositorioDapper;
        private readonly SqlConnection _sqlConnection;

        public UsuarioDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task ActualizarUltimoAcceso(string correo)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            await conexion.ExecuteAsync(
                "dbo.ActualizarUltimoAcceso",
                new { Correo = correo },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<UsuarioResponse> ObtenerInfoUsuario(
            string correo)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QueryFirstOrDefaultAsync<UsuarioResponse>(
                "dbo.ObtenerUsuario",
                new { Correo = correo },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> EditarUsuario(
            int idEmpresa,
            int idUsuario,
            UsuarioEditarRequest usuario, int idUsuarioAccion)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QuerySingleAsync<int>(
                "dbo.EditarUsuario",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    nombre = usuario.Nombre.Trim(),
                    primer_apellido = usuario.PrimerApellido.Trim(),
                    segundo_apellido = usuario.SegundoApellido?.Trim(),
                    correo = usuario.Correo.Trim(),
                    nombre_usuario = usuario.NombreUsuario,
                    id_usuario_accion = idUsuarioAccion
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CambiarEstadoUsuario(
            int idEmpresa,
            int idUsuario,
            bool activo,
            int idUsuarioAccion)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QuerySingleAsync<int>(
                "dbo.CambiarEstadoUsuario",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    activo = activo,
                    id_usuario_accion = idUsuarioAccion
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AsignarRolUsuario(
            int idEmpresa,
            int idUsuario,
            int idRol,
            int idUsuarioAccion)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QuerySingleAsync<int>(
                "dbo.AsignarRolUsuario",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_rol = idRol,
                    id_usuario_accion = idUsuarioAccion
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AprobarRegistroCliente(
            int idEmpresa,
            int idUsuario,
            int idUsuarioAccion,
            bool aprobar)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QuerySingleAsync<int>(
                "dbo.AprobarRegistroCliente",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_usuario_accion = idUsuarioAccion,
                    aprobar = aprobar
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SolicitudClientePendienteResponse>>
            ObtenerSolicitudesClientesPendientes(
                int idEmpresa,
                int idUsuarioAccion)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QueryAsync<SolicitudClientePendienteResponse>(
                "dbo.ObtenerSolicitudesClientesPendientes",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario_accion = idUsuarioAccion
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int?> ObtenerEmpresaUsuarioActivo(
            int idUsuario)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            return await conexion.QuerySingleOrDefaultAsync<int?>(
                "dbo.ObtenerEmpresaUsuarioActivo",
                new { id_usuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UsuarioResponse>> ObtenerUsuarios(
            int idEmpresa)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            var usuarios = await conexion.QueryAsync<UsuarioConRoles>(
                "dbo.ObtenerUsuarios",
                new { id_empresa = idEmpresa },
                commandType: CommandType.StoredProcedure);
            return usuarios.Select(MapearUsuario).ToList();
        }

        public async Task<UsuarioResponse?> ObtenerUsuarioPorId(
            int idEmpresa,
            int idUsuario)
        {
            using var conexion = new SqlConnection(
                _sqlConnection.ConnectionString);

            var usuario = await conexion.QuerySingleOrDefaultAsync<UsuarioConRoles>(
                "dbo.ObtenerUsuarioPorId",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario
                },
                commandType: CommandType.StoredProcedure);
            return usuario == null ? null : MapearUsuario(usuario);
        }

        private sealed class UsuarioConRoles : UsuarioResponse
        {
            [JsonIgnore]
            public string? RolesJson { get; set; }
        }

        private static UsuarioResponse MapearUsuario(UsuarioConRoles usuario)
        {
            if (usuario.RolesJson == null)
                throw new InvalidOperationException("La consulta administrativa debe devolver RolesJson.");

            usuario.Roles = (JsonSerializer.Deserialize<List<UsuarioRolResponse>>(usuario.RolesJson)
                ?? throw new InvalidOperationException("La consulta administrativa devolvio roles invalidos."))
                .DistinctBy(rol => rol.IdRol).ToList();
            var representativo = usuario.Roles.FirstOrDefault();
            usuario.IdRol = representativo?.IdRol;
            usuario.NombreRol = representativo?.NombreRol;
            return usuario;
        }
        public async Task<int> RetirarRolUsuario(int idEmpresa,int idUsuario,int idRol,int idUsuarioAccion)
        {
            using var c=new SqlConnection(_sqlConnection.ConnectionString);
            return await c.QuerySingleAsync<int>("dbo.RetirarRolUsuario",new {id_empresa=idEmpresa,id_usuario=idUsuario,id_rol=idRol,id_usuario_accion=idUsuarioAccion},commandType:CommandType.StoredProcedure);
        }
        public async Task<CatalogosUsuarios> ObtenerCatalogos(int idEmpresa,int idUsuarioAccion)
        {
            using var c=new SqlConnection(_sqlConnection.ConnectionString);
            using var datos=await c.QueryMultipleAsync("dbo.ObtenerCatalogosAdministracionUsuarios",new {id_empresa=idEmpresa,id_usuario_accion=idUsuarioAccion},commandType:CommandType.StoredProcedure);
            var catalogos=new CatalogosUsuarios {Roles=(await datos.ReadAsync<OpcionUsuario>()).ToList(),Estados=(await datos.ReadAsync<OpcionUsuario>()).ToList()};
            datos.Dispose();
            // El ID procede exclusivamente del contexto autenticado del controlador.
            var empresa=await c.QuerySingleOrDefaultAsync<Empresa>("dbo.ObtenerEmpresaPorId",new {id_empresa=idEmpresa},commandType:CommandType.StoredProcedure);
            if(empresa==null || empresa.IdEmpresa!=idEmpresa)throw new InvalidOperationException("Empresa del contexto no disponible.");
            catalogos.NombreEmpresa=string.IsNullOrWhiteSpace(empresa.NombreComercial)?empresa.RazonSocial:empresa.NombreComercial;
            return catalogos;
        }    }
}
