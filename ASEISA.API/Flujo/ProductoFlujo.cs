using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class ProductoFlujo : IProductoFlujo
    {
        private static readonly string[] TiposPermitidos = { "bienes", "servicios", "combo" };
        private static readonly string[] PoliticasPermitidas = { "cantidad ordenada", "cantidad entregada" };

        private IProductoDA _productoDA;

        public ProductoFlujo(IProductoDA productoDA)
        {
            _productoDA = productoDA;
        }

        public async Task<int> RegistrarProducto(Producto producto)
        {
            if (producto == null
                || !producto.IdEmpresa.HasValue
                || producto.IdEmpresa <= 0
                || string.IsNullOrWhiteSpace(producto.Codigo)
                || string.IsNullOrWhiteSpace(producto.Nombre)
                || string.IsNullOrWhiteSpace(producto.TipoProducto)
                || string.IsNullOrWhiteSpace(producto.PoliticaFacturacion)
                || !producto.PrecioVenta.HasValue
                || !producto.PorcentajeImpuestoVenta.HasValue
                || !producto.CostoPromedio.HasValue
                || !producto.PorcentajeImpuestoCompra.HasValue
                || !producto.TiempoEntrega.HasValue
                || !producto.IdCuentaIngreso.HasValue
                || producto.IdCuentaIngreso <= 0
                || !producto.IdCuentaGasto.HasValue
                || producto.IdCuentaGasto <= 0
                || !producto.IdBodega.HasValue
                || producto.IdBodega <= 0
                || !producto.Cantidad.HasValue
                || !producto.IdEstado.HasValue
                || producto.IdEstado <= 0)
            {
                return 0;
            }

            var tipo = producto.TipoProducto.Trim();
            var politica = producto.PoliticaFacturacion.Trim();

            if (!TiposPermitidos.Contains(tipo, StringComparer.OrdinalIgnoreCase)
                || !PoliticasPermitidas.Contains(politica, StringComparer.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (producto.PrecioVenta < 0
                || producto.PorcentajeImpuestoVenta < 0
                || producto.CostoPromedio < 0
                || producto.PorcentajeImpuestoCompra < 0
                || producto.TiempoEntrega < 0
                || producto.Cantidad < 0
                || (producto.Peso.HasValue && producto.Peso < 0)
                || (producto.Volumen.HasValue && producto.Volumen < 0))
            {
                return 2;
            }

            producto.TipoProducto = tipo;
            producto.PoliticaFacturacion = politica;

            return await _productoDA.RegistrarProducto(producto);
        }
    }
}
