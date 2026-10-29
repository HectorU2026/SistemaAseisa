using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA
{
    public class CuentasPorPagarDA : ICuentasPorPagarDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public CuentasPorPagarDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }
        public async Task<IEnumerable<CuentaPorPagar>> ObtenerCuentasPorPagar()
        {
            var sql = @"ObtenerCuentasPorPagas";
            return await _sqlConnection.QueryAsync<CuentaPorPagar>(sql);
        }
    }
}
