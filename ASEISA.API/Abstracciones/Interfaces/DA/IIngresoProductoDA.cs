using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IIngresoProductoDA
    {
        Task<int> RegistrarIngresoProducto(IngresoProducto ingreso);
    }
}
