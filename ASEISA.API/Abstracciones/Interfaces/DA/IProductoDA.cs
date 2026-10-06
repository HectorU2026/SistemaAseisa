using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IProductoDA
    {
        Task<int> RegistrarProducto(Producto producto);
    }
}
