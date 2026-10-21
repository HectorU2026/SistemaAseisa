using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class IngresoProductoFlujo : IIngresoProductoFlujo
    {
        private IIngresoProductoDA _ingresoProductoDA;

        public IngresoProductoFlujo(IIngresoProductoDA ingresoProductoDA)
        {
            _ingresoProductoDA = ingresoProductoDA;
        }

        public async Task<int> RegistrarIngresoProducto(IngresoProducto ingreso)
        {
            if (ingreso == null
                || ingreso.IdEmpresa <= 0
                || ingreso.IdProducto <= 0
                || ingreso.IdBodega <= 0
                || ingreso.IdUsuario <= 0
                || ingreso.IdContacto <= 0
                || ingreso.FechaEntrega == DateTime.MinValue
                || string.IsNullOrWhiteSpace(ingreso.TipoOperacion)
                || string.IsNullOrWhiteSpace(ingreso.Estado)
                || !ingreso.Cantidad.HasValue)
            {
                return 0;
            }

            if (ingreso.Cantidad < 0)
            {
                return 2;
            }

            var tipo = ingreso.TipoOperacion.Trim();
            var estado = ingreso.Estado.Trim();

            if (!tipo.Equals("recepcion", StringComparison.OrdinalIgnoreCase)
                && !tipo.Equals("entrega", StringComparison.OrdinalIgnoreCase))
            {
                return 11;
            }

            if (!estado.Equals("Borrador", StringComparison.OrdinalIgnoreCase)
                && !estado.Equals("Listo", StringComparison.OrdinalIgnoreCase))
            {
                return 11;
            }

            ingreso.TipoOperacion = tipo.Equals("recepcion", StringComparison.OrdinalIgnoreCase)
                ? "recepcion"
                : "entrega";
            ingreso.Estado = estado.Equals("Borrador", StringComparison.OrdinalIgnoreCase)
                ? "Borrador"
                : "Listo";

            return await _ingresoProductoDA.RegistrarIngresoProducto(ingreso);
        }
    }
}
