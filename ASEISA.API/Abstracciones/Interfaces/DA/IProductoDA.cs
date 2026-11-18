using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IProductoDA
    {
        Task<int> RegistrarProducto(Producto producto, int idEmpresa);
        Task<int> EditarProducto(Producto producto, int idEmpresa);
        Task<int> InhabilitarProducto(int idProducto, int idEmpresa);
        Task<ProductoCatalogoRespuesta> ObtenerProductos(ProductoFiltro filtro);
    }
}
