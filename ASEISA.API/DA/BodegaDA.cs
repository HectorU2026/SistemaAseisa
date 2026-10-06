using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA
{
    public class BodegaDA : IBodegaDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public BodegaDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<int> RegistrarBodega(Bodega bodega)
        {
            var sql = @"RegistrarBodega";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_empresa = bodega.IdEmpresa,
                    codigo = bodega.Codigo,
                    nombre = bodega.Nombre,
                    ubicacion = bodega.Ubicacion,
                    id_estado = bodega.IdEstado
                });

            return resultado.FirstOrDefault();
        }
    }
}
