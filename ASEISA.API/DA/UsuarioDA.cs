using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DA
{
    public class UsuarioDA : IUsuarioDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public UsuarioDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task ActualizarUltimoAcceso(string correo)
        {
            var sql = @"ActualizarUltimoAcceso";

            await _sqlConnection.ExecuteAsync(sql, new { Correo = correo });
        }

        public async Task<UsuarioResponse> ObtenerInfoUsuario(string correo)
        {
            var sql = @"InformacionUsuario";

            var resultado = await _sqlConnection.QueryAsync<UsuarioResponse>(sql, new { Correo = correo });

            return resultado.FirstOrDefault();
        }

        public async Task<int> AgregarUsuario(int idEmpresa, UsuarioRequest usuario)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"RegistroUsuario", new
                {
                    id_empresa = idEmpresa,
                    tipo_cliente = usuario.TipoCliente,
                    nombre = usuario.Nombre,
                    primer_apellido = usuario.PrimerApellido,
                    segundo_apellido = usuario.SegundoApellido,
                    correo = usuario.Correo,
                    nombre_usuario = usuario.NombreUsuario,
                    contrasena_hash = usuario.ContrasenaHash,
                    id_estado = usuario.IdEstado,
                    telefono = usuario.Telefono,
                    direccion = usuario.Direccion,
                    identificacion = usuario.Identificacion,
                    nombre_razon_social = usuario.NombreRazonSocial,
                    id_rol = usuario.IdRol
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> EditarUsuario(int idEmpresa, int idUsuario, UsuarioRequest usuario)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"EditarUsuario", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    tipo_cliente = usuario.TipoCliente,
                    nombre = usuario.Nombre,
                    primer_apellido = usuario.PrimerApellido,
                    segundo_apellido = usuario.SegundoApellido,
                    correo = usuario.Correo,
                    nombre_usuario = usuario.NombreUsuario,
                    contrasena_hash = usuario.ContrasenaHash,
                    id_estado = usuario.IdEstado,
                    telefono = usuario.Telefono,
                    direccion = usuario.Direccion,
                    identificacion = usuario.Identificacion,
                    nombre_razon_social = usuario.NombreRazonSocial,
                    id_rol = usuario.IdRol
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        // activar o desactivar usuario
        public async Task<int> CambiarEstadoUsuario(int idEmpresa, int idUsuario, int idEstado)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"CambiarEstadoUsuario", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_estado = idEstado
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        //restablecer contraseña
        public async Task<int> RestablecerContrasenaUsuario(int idEmpresa, int idUsuario, string contrasenaHash)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"RestablecerContrasenaUsuario", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    contrasena_hash = contrasenaHash
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        // asignar rol a usuario
        public async Task<int> AsignarRolUsuario(int idEmpresa, int idUsuario, int idRol)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"AsignarRolUsuario", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_rol = idRol
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int?> ObtenerEmpresaUsuarioActivo(int idUsuario)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);

            return await conexion.QuerySingleOrDefaultAsync<int?>(
                "dbo.ObtenerEmpresaUsuarioActivo",
                new
                {
                    id_usuario = idUsuario
                },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}