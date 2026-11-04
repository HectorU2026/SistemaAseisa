using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class ActivoFijoFlujo : IActivoFijoFlujo
    {
        private IActivoFijoDA _activoFijoDA;

        public ActivoFijoFlujo(IActivoFijoDA activoFijoDA)
        {
            _activoFijoDA = activoFijoDA;
        }
        public async Task<int> RegistrarActivoFijo(ActivoFijoRequest activo)
        {
            if (activo == null
                || !activo.IdEmpresa.HasValue || activo.IdEmpresa <= 0
                || !activo.IdUsuario.HasValue || activo.IdUsuario <= 0
                || string.IsNullOrWhiteSpace(activo.Codigo)
                || string.IsNullOrWhiteSpace(activo.Nombre)
                || !activo.FechaAdquisicion.HasValue
                || !activo.CostoAdquisicion.HasValue
                || !activo.ValorResidual.HasValue
                || !activo.VidaUtilMeses.HasValue
                || string.IsNullOrWhiteSpace(activo.MetodoDepreciacion)
                || !activo.IdCuentaActivo.HasValue || activo.IdCuentaActivo <= 0
                || !activo.IdCuentaDepreciacion.HasValue || activo.IdCuentaDepreciacion <= 0
                || !activo.IdEstado.HasValue || activo.IdEstado <= 0)
            {
                return 0; // Incompleto
            }

            if (activo.CostoAdquisicion < 0
                || activo.ValorResidual < 0
                || activo.VidaUtilMeses <= 0)
            {
                return 2; // Valores negativos
            }

            return await _activoFijoDA.RegistrarActivoFijo(activo);
        }

        public async Task<int> CalcularDepreciacionActivo(CalcularDepreciacionRequest request)
        {
            // Validaciones básicas de campos obligatorios
            if (request == null
                || !request.IdEmpresa.HasValue || request.IdEmpresa <= 0
                || !request.IdActivoFijo.HasValue || request.IdActivoFijo <= 0
                || string.IsNullOrWhiteSpace(request.Periodo)
                || !request.IdEstado.HasValue || request.IdEstado <= 0
                || !request.IdUsuario.HasValue || request.IdUsuario <= 0)
            {
                return 0; // Campos incompletos
            }

            return await _activoFijoDA.CalcularDepreciacionActivo(request);
        }

        public async Task<IEnumerable<DepreciacionHistorialResponse>> ObtenerHistorialDepreciacion(int idEmpresa, int idActivoFijo)
        {
            if (idEmpresa <= 0 || idActivoFijo <= 0)
            {
                return Enumerable.Empty<DepreciacionHistorialResponse>();
            }

            return await _activoFijoDA.ObtenerHistorialDepreciacion(idEmpresa, idActivoFijo);
        }
    }
}
