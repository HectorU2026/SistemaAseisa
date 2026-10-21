using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class InventarioFisicoFlujo : IInventarioFisicoFlujo
    {
        private IInventarioFisicoDA _inventarioFisicoDA;

        public InventarioFisicoFlujo(IInventarioFisicoDA inventarioFisicoDA)
        {
            _inventarioFisicoDA = inventarioFisicoDA;
        }

        public async Task<int> RegistrarInventarioFisico(InventarioFisico inventario)
        {
            if (inventario == null
                || !inventario.IdEmpresa.HasValue
                || inventario.IdEmpresa <= 0
                || !inventario.IdProducto.HasValue
                || inventario.IdProducto <= 0
                || !inventario.IdBodega.HasValue
                || inventario.IdBodega <= 0
                || !inventario.IdUsuario.HasValue
                || inventario.IdUsuario <= 0
                || !inventario.CantidadDelInventario.HasValue
                || !inventario.ContadoFisicamente.HasValue)
            {
                return 0;
            }

            if (inventario.ContadoFisicamente < 0)
            {
                return 2;
            }

            return await _inventarioFisicoDA.RegistrarInventarioFisico(inventario);
        }

        public async Task<ExistenciaConsulta?> ObtenerExistencia(ExistenciaFiltro filtro)
        {
            if (filtro == null
                || !filtro.IdEmpresa.HasValue
                || filtro.IdEmpresa <= 0
                || !filtro.IdProducto.HasValue
                || filtro.IdProducto <= 0
                || !filtro.IdBodega.HasValue
                || filtro.IdBodega <= 0)
            {
                return null;
            }

            return await _inventarioFisicoDA.ObtenerExistencia(filtro);
        }
    }
}
