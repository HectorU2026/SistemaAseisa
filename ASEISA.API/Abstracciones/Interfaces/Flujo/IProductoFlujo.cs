using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IProductoFlujo
    {
        Task<int> RegistrarProducto(Producto producto, int idEmpresa);
        Task<int> EditarProducto(Producto producto, int idEmpresa);
        Task<int> InhabilitarProducto(ProductoInhabilitar producto, int idEmpresa);
        Task<ProductoCatalogoRespuesta> ObtenerProductos(ProductoFiltro filtro);
    }
}
