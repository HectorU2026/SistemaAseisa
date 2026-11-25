using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DA
{
    public class ReportesDA : IReportesDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public ReportesDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }


        public async Task<BalanceGeneralResponse> ObtenerBalanceGeneral(int idEmpresa, BalanceGeneralFiltro filtro)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);

            var datosFiltro = new
            {
                id_empresa = idEmpresa,
                fecha_corte_1 = filtro.FechaCorte1,
                fecha_corte_2 = filtro.FechaCorte2,
                incluir_sin_mov = filtro.IncluirSinMovimiento
            };

            var detalle = await conexion.QueryAsync<BalanceGeneralDetalle>(
                "ObtenerBalanceGeneralDetalle",
                datosFiltro,
                commandType: CommandType.StoredProcedure);

            var totales = await conexion.QueryAsync<BalanceGeneralTotal>(
                "ObtenerBalanceGeneralTotales",
                datosFiltro,
                commandType: CommandType.StoredProcedure);

            return new BalanceGeneralResponse
            {
                Detalle = detalle.ToList(),
                Totales = totales.ToList()
            };
        }

        public async Task<LibroMayorResponse> ObtenerLibroMayor(int idEmpresa, LibroMayorRequest request)
        {
            var sql = "ObtenerLibroMayor";

            using var multi = await _sqlConnection.QueryMultipleAsync(
                sql,
                new
                {
                    id_empresa = idEmpresa,
                    id_cuenta_contable = request.IdCuentaContable,
                    fecha_inicio = request.FechaInicio,
                    fecha_fin = request.FechaFin
                },
                commandType: CommandType.StoredProcedure);

            return new LibroMayorResponse
            {
                Cuenta = await multi.ReadSingleAsync<LibroMayorEncabezado>(),
                Movimientos = (await multi.ReadAsync<LibroMayorMovimiento>()).ToList()
            };
        }


        public async Task<ReporteCuentaPorPagarResponse> ObtenerReporteCuentaPorPagar(int idEmpresa, ReporteCuentaPorPagarRequest request)
        {
            var sql = "ObtenerReporteCuentasPorPagar";

            var parametros = new
            {
                id_empresa = idEmpresa,
                id_proveedor = request.IdProveedor,
                solo_vencidas = request.SoloVencidas,
                fecha_corte = request.FechaCorte ?? DateTime.Today
            };

            var listaPlana = (await _sqlConnection.QueryAsync<ReporteCuentaPorPagarDetalle>(
                sql,
                parametros)).ToList();

            var proveedoresAgrupados = listaPlana
                .GroupBy(x => new { x.IdProveedor, x.Proveedor })
                .Select(g => new ReporteCuentaPorPagarProveedor
                {
                    IdProveedor = g.Key.IdProveedor,
                    NombreProveedor = g.Key.Proveedor,
                    TotalAdeudado = g.Sum(x => x.SaldoPendiente),
                    TotalVencido = g.Where(x => x.EstadoVencimiento == "Vencida").Sum(x => x.SaldoPendiente),
                    Facturas = g.ToList()
                })
                .OrderBy(x => x.NombreProveedor)
                .ToList();

            var response = new ReporteCuentaPorPagarResponse
            {
                Proveedores = proveedoresAgrupados,
                Resumen = new ReporteCuentaPorPagarResumen
                {
                    FechaCorte = parametros.fecha_corte,
                    CantidadFacturas = listaPlana.Count,
                    TotalPorPagar = listaPlana.Sum(x => x.SaldoPendiente),
                    TotalVencido = listaPlana.Where(x => x.EstadoVencimiento == "Vencida").Sum(x => x.SaldoPendiente),
                    TotalPorVencer = listaPlana.Where(x => x.EstadoVencimiento == "Por Vencer").Sum(x => x.SaldoPendiente)
                }
            };

            if (!listaPlana.Any())
            {
                response.Resumen.Mensaje = "No hay cuentas por pagar registradas para los filtros seleccionados.";
            }

            return response;
        }
    }
}
    


