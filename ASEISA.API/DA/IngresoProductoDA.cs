using System.Data;
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA
{
    public class IngresoProductoDA : IIngresoProductoDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public IngresoProductoDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<int> RegistrarIngresoProducto(IngresoProducto ingreso)
        {
            var sql = @"RegistrarIngresoProducto";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_empresa = ingreso.IdEmpresa,
                    id_producto = ingreso.IdProducto,
                    id_bodega = ingreso.IdBodega,
                    id_usuario = ingreso.IdUsuario,
                    id_contacto = ingreso.IdContacto,
                    fecha_entrega = ingreso.FechaEntrega,
                    tipo_operacion = ingreso.TipoOperacion,
                    cantidad = ingreso.Cantidad,
                    cotizacion_compra = string.IsNullOrWhiteSpace(ingreso.CotizacionCompra)
                        ? null
                        : ingreso.CotizacionCompra.Trim(),
                    id_cotizacion = ingreso.IdCotizacion,
                    id_distribucion_analitica = ingreso.IdDistribucionAnalitica,
                    estado = ingreso.Estado
                },
                commandType: CommandType.StoredProcedure);

            var codigos = resultado.ToList();
            if (codigos.Count != 1)
            {
                throw new InvalidOperationException(
                    "El procedimiento RegistrarIngresoProducto no devolvió exactamente un código de resultado.");
            }

            return codigos[0];
        }
    }
}
