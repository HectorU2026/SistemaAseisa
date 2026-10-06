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
            var sql = @"RegistroUsuario";

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
                    id_estado = infoUsuario.Usuario.IdEstado,
                    telefono = infoUsuario.Cliente.Telefono,
                    direccion = infoUsuario.Cliente.Direccion,
                    identificacion = infoUsuario.Cliente.Identificacion,
                    nombre_razon_social = infoUsuario.Cliente.NombreRazonSocial,
                    id_rol = 3
                });
            return new RegistroResponse
            {
                IdUsuario = resultado,
                mensaje = "Usuario registrado correctamente"
            }; ;
        }

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            string sql = @"ObtenerUsuario";
            var resultado = await _sqlConnection.QueryAsync<LoginAutenticado>(sql, 
            new { 
                correo = login.Correo
            });
            return resultado.FirstOrDefault();
        }

        public async Task<ActionResult> CambiarContrasena(CambiarContrasena usuario)
        {
            string sql = @"CambiarContrasena";
            var resultado = await _sqlConnection.QueryAsync<CambiarContrasena>(sql,
            new
            {
                correo = usuario.correo,
                contrasena = usuario.ContrasenaHash
            });
            return new OkResult();
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
            string sql = @"ValidarCorreo";
            var resultado = await _sqlConnection.ExecuteScalarAsync<int>(sql,
                new { 
                    correo = correo 
                });

            return resultado;
        }

        private async Task<int> ValidarNombreUsuario(string nombreUsuario)
        {
            string sql = @"ValidarNombreUsuario";
            var resultado = await _sqlConnection.ExecuteScalarAsync<int>(sql,
                new { 
                    nombre_usuario = nombreUsuario 
                });

            return resultado;
        }

    }
}
