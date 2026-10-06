using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA
{
    public class EmpresaDA : IEmpresaDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public EmpresaDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }
        public async Task<IEnumerable<EmpresaResponseNombres>> ObtenerNombreEmpresa()
        {
            var sql = @"ObtenerNombreEmpresa";

            var resultado = await _sqlConnection.QueryAsync<EmpresaResponseNombres>(sql);
            return resultado;
        }

    }
}
