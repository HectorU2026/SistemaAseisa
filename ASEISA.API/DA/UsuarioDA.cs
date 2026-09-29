using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

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

        public async Task<Usuario> ObtenerInfoUsuario(string correo)
        {
            var sql = @"ObtenerUsuario";

            var resultado = await _sqlConnection.QueryAsync<Usuario>(sql, new { Correo = correo });

            return resultado.FirstOrDefault();
        }


    }
}
