using Abstracciones.DA;
using Abstracciones.Modelos;
using Dapper;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace DA
{
    public class UsuarioDA : IUsuarioDA
    { 
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public UsuarioDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorioDapper();
        }

        public async Task<RegistroResponse> CrearUsuario(RegistroRequest infoUsuario)
        {
            var existeCorreo = await ValidarCorreo(infoUsuario.Usuario.Correo);
            if (existeCorreo > 0)
            {
                return new RegistroResponse
                {
                    IdUsuario = null,
                    mensaje = "El correo ya fue registrado"
                };
            }

            var existeUsuario = await ValidarNombreUsuario(infoUsuario.Usuario.NombreUsuario);
            if (existeUsuario > 0)
            {
                return new RegistroResponse
                {
                    IdUsuario = null,
                    mensaje = "El nombre de usuario ya fue registrado"
                };
            }
            var sql = @"dbo.RegistroUsuario";

            var resultado = await _sqlConnection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    id_empresa = infoUsuario.Usuario.IdEmpresa,
                    tipo_cliente = infoUsuario.Usuario.TipoCliente,
                    nombre = infoUsuario.Usuario.Nombre,
                    primer_apellido = infoUsuario.Usuario.PrimerApellido,
                    segundo_apellido = infoUsuario.Usuario.SegundoApellido,
                    correo = infoUsuario.Usuario.Correo,
                    nombre_usuario = infoUsuario.Usuario.NombreUsuario,
                    contrasena_hash = infoUsuario.Usuario.ContrasenaHash,
                    id_estado = (int?)null, // SQL fuerza Inactivo para el registro publico.
                    telefono = ASEISA.Usuarios.ReglasRegistroUsuario.NormalizarTelefono(infoUsuario.Cliente.Telefono),
                    direccion = infoUsuario.Cliente.Direccion,
                    identificacion = ASEISA.Usuarios.ReglasRegistroUsuario.NormalizarIdentificacion(infoUsuario.Cliente.Identificacion),
                    nombre_razon_social = infoUsuario.Cliente.NombreRazonSocial,
                    id_rol = (int?)null, // SQL resuelve exclusivamente Cliente.
                    registro_publico = true,
                    id_usuario_accion = (int?)null
                }, commandType: CommandType.StoredProcedure);
            return new RegistroResponse
            {
                IdUsuario = resultado,
                mensaje = "Usuario registrado correctamente"
            }; ;
        }

        public async Task<IEnumerable<LoginAutenticado>> ObtenerCandidatoAutenticacion(LoginBase login)
            => await _sqlConnection.QueryAsync<LoginAutenticado>("dbo.ObtenerCandidatoAutenticacion",
                new {correo=login.Correo},commandType:CommandType.StoredProcedure);

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            var filas = await ObtenerUsuarioConRoles(login);
            return filas.FirstOrDefault();
        }

        public async Task<IEnumerable<LoginAutenticado>> ObtenerUsuarioConRoles(LoginBase login)
        {
            string sql = @"ObtenerUsuario";
            var resultado = await _sqlConnection.QueryAsync<LoginAutenticado>(sql,
            new {
                correo = login.Correo
            });
            return resultado;
        }

        public async Task<bool> MigrarHashAutenticacion(LoginAutenticado usuario, string hashNuevo)
        {
            using var c=new SqlConnection(_sqlConnection.ConnectionString);
            return await c.QuerySingleAsync<bool>("dbo.MigrarHashAutenticacion",new {
                id_usuario=usuario.IdUsuario,version_credencial=usuario.VersionCredencial,
                hash_anterior=usuario.ContrasenaHash,hash_nuevo=hashNuevo
            },commandType:CommandType.StoredProcedure);
        }
        public async Task<UsuarioRecuperar> ValidarCorreoActivo(string correo)
        {
            string sql = @"ValidarCorreoActivo";
            var resultado = await _sqlConnection.QueryAsync<UsuarioRecuperar>(sql,
                new
                {
                    correo = correo
                });

            return resultado.FirstOrDefault();
        }

        private async Task<int> ValidarCorreo(string correo)
        {
            string sql = @"dbo.ValidarCorreo";
            var resultado = await _sqlConnection.ExecuteScalarAsync<int>(sql,
                new { 
                    correo = correo 
                }, commandType: CommandType.StoredProcedure);

            return resultado;
        }

        private async Task<int> ValidarNombreUsuario(string nombreUsuario)
        {
            string sql = @"dbo.ValidarNombreUsuario";
            var resultado = await _sqlConnection.ExecuteScalarAsync<int>(sql,
                new { 
                    nombre_usuario = nombreUsuario 
                }, commandType: CommandType.StoredProcedure);

            return resultado;
        }

    }
}
