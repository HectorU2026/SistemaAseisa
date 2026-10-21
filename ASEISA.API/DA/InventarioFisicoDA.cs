using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA
{
    public class InventarioFisicoDA : IInventarioFisicoDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public InventarioFisicoDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<int> RegistrarInventarioFisico(InventarioFisico inventario)
        {
            var sql = @"RegistrarInventarioFisico";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_empresa = inventario.IdEmpresa,
                    id_producto = inventario.IdProducto,
                    id_bodega = inventario.IdBodega,
                    id_usuario = inventario.IdUsuario,
                    cantidad_inventario = inventario.CantidadDelInventario,
                    contado_fisicamente = inventario.ContadoFisicamente
                });

            return resultado.FirstOrDefault();
        }

        public async Task<ExistenciaConsulta?> ObtenerExistencia(ExistenciaFiltro filtro)
        {
            var sql = @"ObtenerExistencia";

            return await _sqlConnection.QueryFirstOrDefaultAsync<ExistenciaConsulta>(sql,
                new
                {
                    id_empresa = filtro.IdEmpresa,
                    id_producto = filtro.IdProducto,
                    id_bodega = filtro.IdBodega
                });
        }
    }
}
