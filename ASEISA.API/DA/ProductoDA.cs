using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA
{
    public class ProductoDA : IProductoDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public ProductoDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<int> RegistrarProducto(Producto producto)
        {
            var sql = @"RegistrarProducto";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_empresa = producto.IdEmpresa,
                    codigo = producto.Codigo,
                    nombre = producto.Nombre,
                    descripcion = producto.Descripcion,
                    tipo_producto = producto.TipoProducto,
                    costo_promedio = producto.CostoPromedio,
                    precio_venta = producto.PrecioVenta,
                    id_estado = producto.IdEstado,
                    politica_facturacion = producto.PoliticaFacturacion,
                    porcentaje_impuesto_venta = producto.PorcentajeImpuestoVenta,
                    porcentaje_impuesto_compra = producto.PorcentajeImpuestoCompra,
                    peso = producto.Peso,
                    volumen = producto.Volumen,
                    tiempo_entrega = producto.TiempoEntrega,
                    id_cuenta_ingreso = producto.IdCuentaIngreso,
                    id_cuenta_gasto = producto.IdCuentaGasto,
                    id_bodega = producto.IdBodega,
                    cantidad = producto.Cantidad
                });

            return resultado.FirstOrDefault();
        }

        public async Task<int> EditarProducto(Producto producto)
        {
            var sql = @"EditarProducto";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_producto = producto.IdProducto,
                    nombre = producto.Nombre,
                    descripcion = producto.Descripcion,
                    tipo_producto = producto.TipoProducto,
                    costo_promedio = producto.CostoPromedio,
                    precio_venta = producto.PrecioVenta,
                    id_estado = producto.IdEstado,
                    politica_facturacion = producto.PoliticaFacturacion,
                    porcentaje_impuesto_venta = producto.PorcentajeImpuestoVenta,
                    porcentaje_impuesto_compra = producto.PorcentajeImpuestoCompra,
                    peso = producto.Peso,
                    volumen = producto.Volumen,
                    tiempo_entrega = producto.TiempoEntrega,
                    id_cuenta_ingreso = producto.IdCuentaIngreso,
                    id_cuenta_gasto = producto.IdCuentaGasto,
                    id_bodega = producto.IdBodega
                });

            return resultado.FirstOrDefault();
        }

        public async Task<int> InhabilitarProducto(int idProducto)
        {
            var sql = @"InhabilitarProducto";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_producto = idProducto
                });

            return resultado.FirstOrDefault();
        }
    }
}
