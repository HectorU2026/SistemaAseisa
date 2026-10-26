using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IFacturaFlujo
    {
        Task<ActionResult> ObtenerFacturas(int idUsuario, FacturaFiltro? filtro);
        Task<ActionResult> ObtenerFactura(int idUsuario, int idFactura);
    }
}
