using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IProductoFlujo
    {
        Task<int> RegistrarProducto(Producto producto);
        Task<int> EditarProducto(Producto producto);
        Task<int> InhabilitarProducto(ProductoInhabilitar producto);
        Task<ProductoCatalogoRespuesta> ObtenerProductos(ProductoFiltro filtro);
    }
}
