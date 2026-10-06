using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class BodegaFlujo : IBodegaFlujo
    {
        private IBodegaDA _bodegaDA;

        public BodegaFlujo(IBodegaDA bodegaDA)
        {
            _bodegaDA = bodegaDA;
        }

        public async Task<int> RegistrarBodega(Bodega bodega)
        {
            if (bodega == null
                || bodega.IdEmpresa <= 0
                || bodega.IdEstado <= 0
                || string.IsNullOrWhiteSpace(bodega.Codigo)
                || string.IsNullOrWhiteSpace(bodega.Nombre)
                || string.IsNullOrWhiteSpace(bodega.Ubicacion))
            {
                return 0;
            }

            return await _bodegaDA.RegistrarBodega(bodega);
        }
    }
}
