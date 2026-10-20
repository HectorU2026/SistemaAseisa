using System.Data;
using System.Text.Json;
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA;

public class CompraDA : ICompraDA
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _opcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CompraDA(IRepositorioDapper repositorioDapper)
    {
        _connectionString = repositorioDapper.ObtenerRepositorio().ConnectionString;
    }

    #region "Operaciones"

    public async Task<CompraGuardadaResponse> Agregar(int idEmpresa, int idUsuario, CompraRequest compra)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<CompraGuardadaResponse>("dbo.AgregarCompra", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario,
            id_proveedor = compra.IdProveedor,
            productos = JsonSerializer.Serialize(compra.Productos, _opcionesJson),
            id_termino_pago = compra.IdTerminoPago,
            porcentaje_descuento = compra.PorcentajeDescuento,
            confirmacion_llegada = compra.ConfirmacionLlegada,
            id_estado = compra.IdEstado,
            fecha_requerida = compra.FechaRequerida
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<CompraGuardadaResponse> Editar(int idEmpresa, int idUsuario, int idCompra, CompraRequest compra)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<CompraGuardadaResponse>("dbo.EditarCompra", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario,
            id_compra = idCompra,
            id_proveedor = compra.IdProveedor,
            productos = JsonSerializer.Serialize(compra.Productos, _opcionesJson),
            id_termino_pago = compra.IdTerminoPago,
            porcentaje_descuento = compra.PorcentajeDescuento,
            confirmacion_llegada = compra.ConfirmacionLlegada,
            id_estado = compra.IdEstado,
            fecha_requerida = compra.FechaRequerida
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Anular(int idEmpresa, int idUsuario, int idCompra)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.ExecuteScalarAsync<int>("dbo.AnularCompra", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario,
            id_compra = idCompra
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<CompraPaginaResponse> Obtener(int idEmpresa, int idUsuario, CompraFiltro filtro)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var resultados = await conexion.QueryMultipleAsync("dbo.ObtenerCompras", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario,
            busqueda = filtro.Busqueda,
            id_estado = filtro.IdEstado,
            pagina = filtro.Pagina,
            tamano_pagina = filtro.TamanoPagina
        }, commandType: CommandType.StoredProcedure);

        var compras = (await resultados.ReadAsync<CompraListadoDA>()).ToList();
        var paginacion = await resultados.ReadSingleAsync<CompraPaginacionResponse>();

        foreach (var compra in compras)
        {
            compra.Productos = JsonSerializer.Deserialize<List<CompraProductoResponse>>(
                compra.ProductosJson, _opcionesJson) ?? new();
        }

        return new CompraPaginaResponse
        {
            Compras = compras.Cast<CompraResponse>().ToList(),
            Paginacion = paginacion
        };
    }

    public async Task<CompraResponse?> Obtener(int idEmpresa, int idUsuario, int idCompra)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var resultados = await conexion.QueryMultipleAsync("dbo.ObtenerCompra", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario,
            id_compra = idCompra
        }, commandType: CommandType.StoredProcedure);

        var compra = await resultados.ReadSingleOrDefaultAsync<CompraResponse>();
        var productos = (await resultados.ReadAsync<CompraProductoResponse>()).ToList();

        if (compra != null)
            compra.Productos = productos;

        return compra;
    }

    public async Task<CompraCatalogosResponse> ObtenerCatalogos(int idEmpresa, int idUsuario)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var resultados = await conexion.QueryMultipleAsync("dbo.ObtenerCatalogosCompra", new
        {
            id_empresa = idEmpresa,
            id_usuario = idUsuario
        }, commandType: CommandType.StoredProcedure);

        var proveedores = (await resultados.ReadAsync<CompraProveedorCatalogoResponse>()).ToList();
        var productos = (await resultados.ReadAsync<CompraProductoCatalogoResponse>()).ToList();
        var terminos = (await resultados.ReadAsync<CompraTerminoPagoCatalogoResponse>()).ToList();
        var estados = (await resultados.ReadAsync<CompraEstadoCatalogoResponse>()).ToList();

        return new CompraCatalogosResponse
        {
            Proveedores = proveedores,
            Productos = productos,
            TerminosPago = terminos,
            Estados = estados
        };
    }

    #endregion

    #region "Modelos internos"

    private class CompraListadoDA : CompraResponse
    {
        public string ProductosJson { get; set; } = "[]";
    }

    #endregion
}
