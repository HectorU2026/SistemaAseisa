using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Abstracciones.Modelos.Servicios;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using static Abstracciones.Modelos.CotizacionResponse;
using static Abstracciones.Modelos.Facturacion;

namespace DA
{
    public class CotizacionDA : ICotizacionDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public CotizacionDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }
        public async Task<int> RegistrarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            var detallesJson = JsonSerializer.Serialize(cotizacion.Detalles);

            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"RegistrarCotizacion", new
                {
                    id_empresa = idEmpresa,
                    id_cliente = cotizacion.IdCliente,
                    numero_cotizacion = cotizacion.NumeroCotizacion,
                    fecha_emision = cotizacion.FechaEmision,
                    fecha_vencimiento = cotizacion.FechaVencimiento,
                    moneda = cotizacion.Moneda,
                    tipo_cambio = cotizacion.TipoCambio,
                    subtotal = cotizacion.Subtotal,
                    impuesto = cotizacion.Impuesto,
                    descuento = cotizacion.Descuento,
                    total = cotizacion.Total,
                    id_estado = cotizacion.IdEstado,
                    id_usuario = idUsuario,
                    detalles_json = detallesJson
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> AvanzarCotizacionEnviada(int idEmpresa, int idUsuario, int idCotizacion)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"AvanzarCotizacionEnviada", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_cotizacion = idCotizacion
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<CotizacionResponseDetallada?> ObtenerCotizacionPorId(int idCotizacion)
        {
            var sql = @"ObtenerCotizacionPorId";

            using var multi = await _sqlConnection.QueryMultipleAsync(sql,
                new
                {
                    id_cotizacion = idCotizacion
                });

            var cotizacion = await multi.ReadFirstOrDefaultAsync<CotizacionResponseDetallada>();
            var empresa = await multi.ReadFirstOrDefaultAsync<CotizacionResponseDetallada.EmpresaCotizacion>();
            var cliente = await multi.ReadFirstOrDefaultAsync<CotizacionResponseDetallada.ClienteCotizacion>();

            cotizacion.Empresa = empresa;
            cotizacion.Cliente = cliente;
            cotizacion.Detalles = (await multi.ReadAsync<CotizacionResponseDetallada.DetalleCotizacion>()).ToList();

            return cotizacion;
        }

        public async Task<int> RetrocederCotizacionEnviada(int idCotizacion)
        {
            var sql = @"RetrocederCotizacionEnviada";

            return await _sqlConnection.QuerySingleAsync<int>(sql,
                new
                {
                    id_cotizacion = idCotizacion
                });;
        }

        public async Task<int> AvanzarCotizacionAOrdenDeVenta(int idCotizacion)
        {
            var sql = @"AvanzarCotizacionAOrdenDeVenta";

            return await _sqlConnection.QuerySingleAsync<int>(sql,
                new
                {
                    id_cotizacion = idCotizacion
                }); 
        }

        public async Task<FacturacionResponse?> FacturarOrdenDeVenta(FacturarCotizacion factura)
        {
            var sql = "FacturarOrdenDeVenta";

            var resultado = await _sqlConnection.QueryFirstOrDefaultAsync<FacturacionResponse>(
                sql,
                new
                {
                    id_cotizacion = factura.IdCotizacion,
                    id_empresa = factura.IdEmpresa,
                    id_cliente = factura.IdCliente,
                    id_usuario = factura.IdUsuario,
                    numero_venta = factura.NumeroVenta,
                    numero_factura = (string?)null,
                    tipo_factura = factura.TipoFactura,
                    condicion_venta = factura.CondicionVenta,
                    moneda = factura.Moneda,
                    tipo_cambio = factura.TipoCambio,
                    subtotal = factura.Subtotal,
                    impuesto = factura.Impuesto,
                    descuento = factura.Descuento,
                    total = factura.Total
                },
                commandType: CommandType.StoredProcedure);

            return resultado;
        }


        public async Task<IEnumerable<CotizacionResponseDetallada>> ObtenerCanceladasRechazadas()
        {
            var sql = "ObtenerCotizacionesCanceladasRechazadas";

            using var multi = await _sqlConnection.QueryMultipleAsync(sql);

            var cotizaciones = (await multi.ReadAsync<CotizacionResponseDetallada>()).ToList();
            var detalles = (await multi.ReadAsync<CotizacionResponseDetallada.DetalleCotizacion>()).ToList();

            return cotizaciones.Select(c => {
                c.Detalles = detalles.Where(d => d.IdCotizacion == c.IdCotizacion).ToList();
                return c;
            }).ToList();
        }


        public async Task<IEnumerable<FaltanteInventarioResponse>> ValidarInventarioCotizacion(int idCotizacion)
        {
            var sql = "ValidarInventarioCotizacion";

            var resultado = await _sqlConnection.QueryAsync<FaltanteInventarioResponse>(
                sql,
                new { id_cotizacion = idCotizacion });
            return resultado;
        }
        public async Task<IEnumerable<CotizacionResponse>> ObtenerCotizaciones(int idEmpresa)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            var resultado = await conexion.QueryAsync<CotizacionResponse>(
                @"ObtenerCotizaciones",
                new { id_empresa = idEmpresa },
        commandType: System.Data.CommandType.StoredProcedure);

            return resultado;
        }

        public async Task<int> EditarCotizacion(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            var detallesJson = JsonSerializer.Serialize(cotizacion.Detalles);

            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"EditarCotizacion", new
                {
                    id_cotizacion = cotizacion.IdCotizacion,
                    id_empresa = idEmpresa,
                    id_cliente = cotizacion.IdCliente,
                    fecha_vencimiento = cotizacion.FechaVencimiento,
                    moneda = cotizacion.Moneda,
                    tipo_cambio = cotizacion.TipoCambio,
                    subtotal = cotizacion.Subtotal,
                    impuesto = cotizacion.Impuesto,
                    descuento = cotizacion.Descuento,
                    total = cotizacion.Total,
                    id_usuario = idUsuario,
                    detalles_json = detallesJson
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CancelarCotizacion(int idEmpresa, int idUsuario, int idCotizacion)
        {
            using var conexion = new SqlConnection(_sqlConnection.ConnectionString);
            return await conexion.QuerySingleAsync<int>(
                @"CancelarCotizacion", new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_cotizacion = idCotizacion
                }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
