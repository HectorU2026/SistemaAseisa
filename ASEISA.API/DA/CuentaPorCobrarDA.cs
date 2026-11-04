using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA
{
    public class CuentaPorCobrarDA : ICuentaPorCobrarDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public CuentaPorCobrarDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }
        public async Task<IEnumerable<CuentaPorCobrarResponse>> ObtenerCuentasPorCobrar(int idEmpresa, int idUsuario)
        {
            var sql = @"ObtenerCuentasPorCobrar";

            var resultado = await _sqlConnection.QueryAsync<CuentaPorCobrarResponse>(sql,
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario
                }, commandType: System.Data.CommandType.StoredProcedure);

            return resultado;
        }
    }
}
