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

        public async Task<IEnumerable<FlujoFacturaListado>> ObtenerFlujoFacturas(int idEmpresa, FlujoFacturaFiltro? filtro)
        {
            var resultado = await _sqlConnection.QueryAsync<FlujoFacturaListado>(
                "ObtenerFlujoFacturas",
                new
                {
                    id_empresa = idEmpresa,
                    id_cotizacion = filtro?.IdCotizacion,
                    numero_factura = filtro?.NumeroFactura,
                    ordenar_por = filtro?.OrdenarPor
                },
                commandType: CommandType.StoredProcedure);

            return resultado;
        }

        public async Task<ReporteFacturacionResultado> ObtenerReporteFacturacion(int idEmpresa, ReporteFacturacionFiltro? filtro)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            using var resultados = await conexion.QueryMultipleAsync(
                "ObtenerReporteFacturacion",
                new
                {
                    id_empresa = idEmpresa,
                    periodo = filtro?.Periodo,
                    tipo_factura = filtro?.TipoFactura,
                    ordenar_por = filtro?.OrdenarPor
                },
                commandType: CommandType.StoredProcedure);

            var grupos = (await resultados.ReadAsync<ReporteFacturacionGrupo>()).ToList();
            var lineas = (await resultados.ReadAsync<ReporteFacturacionLinea>()).ToList();

            var lineasPorGrupo = new Dictionary<ClaveGrupoReporte, List<ReporteFacturacionLinea>>();

            foreach (var grupo in grupos)
            {
                var clave = ClaveGrupoReporte.Desde(grupo.Periodo, grupo.TipoFactura, grupo.Moneda);
                lineasPorGrupo[clave] = new List<ReporteFacturacionLinea>();
            }

            foreach (var linea in lineas)
            {
                var clave = ClaveGrupoReporte.Desde(linea.Periodo, linea.TipoFactura, linea.Moneda);

                if (!lineasPorGrupo.TryGetValue(clave, out var facturasGrupo))
                {
                    throw new InvalidOperationException(
                        $"La factura {linea.IdFactura} no coincide con ningún grupo del reporte (periodo, tipo_factura, moneda).");
                }

                if (facturasGrupo.Exists(factura => factura.IdFactura == linea.IdFactura))
                {
                    throw new InvalidOperationException(
                        $"El reporte devolvió la factura {linea.IdFactura} duplicada en el mismo grupo.");
                }

                facturasGrupo.Add(linea);
            }

            foreach (var grupo in grupos)
            {
                var clave = ClaveGrupoReporte.Desde(grupo.Periodo, grupo.TipoFactura, grupo.Moneda);
                grupo.Facturas = lineasPorGrupo[clave];
            }

            return new ReporteFacturacionResultado
            {
                Grupos = grupos
            };
        }

        public async Task<IEnumerable<ReporteNotaCredito>> ObtenerNotasCreditoReporteFacturacion(int idEmpresa, int idFactura)
        {
            return await _sqlConnection.QueryAsync<ReporteNotaCredito>(
                "ObtenerNotasCreditoReporteFacturacion",
                new
                {
                    id_empresa = idEmpresa,
                    id_factura = idFactura
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ReporteFacturacionDetalle?> ObtenerDetalleReporteFacturacion(int idEmpresa, int idFactura)
        {
            var factura = await ObtenerFactura(idEmpresa, idFactura);

            if (factura == null)
                return null;

            if (!string.Equals(factura.Estado, "Emitida", StringComparison.Ordinal)
                && !string.Equals(factura.Estado, "Pagada", StringComparison.Ordinal))
            {
                return null;
            }

            var notasCredito = (await ObtenerNotasCreditoReporteFacturacion(idEmpresa, idFactura)).ToList();

            return new ReporteFacturacionDetalle
            {
                Factura = factura,
                NotasCredito = notasCredito
            };
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

        public Task<FacturaCambioEstadoResultado> Inactivar(
            int idEmpresa,
            int idUsuario,
            int idFactura,
            byte[] versionFila,
            string motivoAnulacion)
        {
            return EjecutarCambioEstado(
                "InactivarFactura",
                idEmpresa,
                idUsuario,
                idFactura,
                versionFila,
                motivoAnulacion);
        }

        public Task<FacturaCambioEstadoResultado> Reactivar(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila)
        {
            return EjecutarCambioEstado("ReactivarFactura", idEmpresa, idUsuario, idFactura, versionFila);
        }

        public async Task<FacturaPagoResultado> RegistrarPago(int idEmpresa, int idFactura, decimal monto, string metodoPago)
        {
            var parametros = new DynamicParameters();
            parametros.Add("id_factura", idFactura);
            parametros.Add("id_empresa", idEmpresa);
            parametros.Add("monto", monto);
            parametros.Add("metodo_pago", metodoPago);

            return await _sqlConnection.QuerySingleAsync<FacturaPagoResultado>(
                "RegistrarPagoFactura",
                parametros,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<FacturaNotaCreditoResultado> RegistrarNotaCredito(
            int idEmpresa,
            int idUsuario,
            int idFactura,
            string motivo,
            decimal monto,
            string? descripcionAdicional)
        {
            var parametros = new DynamicParameters();
            parametros.Add("id_factura", idFactura);
            parametros.Add("id_empresa", idEmpresa);
            parametros.Add("id_usuario", idUsuario);
            parametros.Add("motivo", motivo);
            parametros.Add("monto", monto);
            parametros.Add("descripcion_adicional", descripcionAdicional);

            return await _sqlConnection.QuerySingleAsync<FacturaNotaCreditoResultado>(
                "RegistrarNotaCredito",
                parametros,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ContarEnvios(int idEmpresa, int idFactura)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(
                "ContarEnviosFactura",
                new
                {
                    id_factura = idFactura,
                    id_empresa = idEmpresa
                },
                commandType: CommandType.StoredProcedure);
        }

        private async Task<FacturaCambioEstadoResultado> EjecutarCambioEstado(
            string procedimiento,
            int idEmpresa,
            int idUsuario,
            int idFactura,
            byte[] versionFila,
            string? motivoAnulacion = null)
        {
            var parametros = new DynamicParameters();
            parametros.Add("id_factura", idFactura);
            parametros.Add("id_empresa", idEmpresa);
            parametros.Add("id_usuario", idUsuario);
            parametros.Add("version_fila", versionFila, DbType.Binary, size: 8);

            if (motivoAnulacion != null)
                parametros.Add("motivo_anulacion", motivoAnulacion);

            var resultado = await _sqlConnection.QuerySingleAsync<FacturaCambioEstadoResultado>(
                procedimiento,
                parametros,
                commandType: CommandType.StoredProcedure);

            return resultado;
        }

        private readonly record struct ClaveGrupoReporte(string Periodo, string? TipoFactura, string? Moneda)
        {
            public static ClaveGrupoReporte Desde(string? periodo, string? tipoFactura, string? moneda)
            {
                return new ClaveGrupoReporte(
                    periodo?.TrimEnd() ?? string.Empty,
                    tipoFactura,
                    moneda);
            }
        }
    }
}
