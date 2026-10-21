using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IIngresoProductoFlujo
    {
        Task<int> RegistrarIngresoProducto(IngresoProducto ingreso);
    }
}
