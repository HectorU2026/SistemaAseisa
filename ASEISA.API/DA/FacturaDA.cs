using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace DA
{
    public class FacturaDA : IFacturaDA
    {
        private readonly SqlConnection _sqlConnection;

        public FacturaDA(IRepositorioDapper repositorioDapper)
        {
            _sqlConnection = repositorioDapper.ObtenerRepositorio();
        }

        public async Task<IEnumerable<FacturaListado>> ObtenerFacturas(int idEmpresa, FacturaFiltro? filtro)
        {
            var resultado = await _sqlConnection.QueryAsync<FacturaListado>(
                "ObtenerFacturas",
                new
                {
                    id_empresa = idEmpresa,
                    id_estado_factura = filtro?.IdEstadoFactura,
                    nombre_cliente = filtro?.NombreCliente
                },
                commandType: CommandType.StoredProcedure);

            return resultado;
        }

        public async Task<FacturaDetalle?> ObtenerFactura(int idEmpresa, int idFactura)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            using var resultados = await conexion.QueryMultipleAsync(
                "ObtenerFactura",
                new
                {
                    id_factura = idFactura,
                    id_empresa = idEmpresa
                },
                commandType: CommandType.StoredProcedure);

            var factura = await resultados.ReadSingleOrDefaultAsync<FacturaDetalle>();
            var productos = (await resultados.ReadAsync<FacturaProducto>()).ToList();
            var asiento = await resultados.ReadSingleOrDefaultAsync<FacturaAsiento>();
            var movimientos = (await resultados.ReadAsync<FacturaMovimientoContable>()).ToList();

            if (factura == null)
                return null;

            factura.Productos = productos;

            if (asiento != null)
            {
                asiento.Movimientos = movimientos;
                factura.Asiento = asiento;
            }

            return factura;
        }
    }
}
