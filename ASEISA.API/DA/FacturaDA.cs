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

        public Task<FacturaCambioEstadoResultado> Confirmar(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila)
        {
            return EjecutarCambioEstado("ConfirmarFactura", idEmpresa, idUsuario, idFactura, versionFila);
        }

        public Task<FacturaCambioEstadoResultado> Restablecer(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila)
        {
            return EjecutarCambioEstado("RestablecerFactura", idEmpresa, idUsuario, idFactura, versionFila);
        }

        private async Task<FacturaCambioEstadoResultado> EjecutarCambioEstado(
            string procedimiento,
            int idEmpresa,
            int idUsuario,
            int idFactura,
            byte[] versionFila)
        {
            var parametros = new DynamicParameters();
            parametros.Add("id_factura", idFactura);
            parametros.Add("id_empresa", idEmpresa);
            parametros.Add("id_usuario", idUsuario);
            parametros.Add("version_fila", versionFila, DbType.Binary, size: 8);

            var resultado = await _sqlConnection.QuerySingleAsync<FacturaCambioEstadoResultado>(
                procedimiento,
                parametros,
                commandType: CommandType.StoredProcedure);

            return resultado;
        }
    }
}
