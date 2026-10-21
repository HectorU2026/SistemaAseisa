using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IInventarioFisicoDA
    {
        Task<int> RegistrarInventarioFisico(InventarioFisico inventario);
        Task<ExistenciaConsulta?> ObtenerExistencia(ExistenciaFiltro filtro);
    }
}
