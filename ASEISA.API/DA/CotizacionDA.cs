using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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
    }
}
