using Abstracciones.Modelos;
using System.Collections.Generic;

namespace Abstracciones.Interfaces.DA
{
    public interface IFacturaDA
    {
        Task<IEnumerable<FacturaListado>> ObtenerFacturas(int idEmpresa, FacturaFiltro? filtro);
        Task<FacturaDetalle?> ObtenerFactura(int idEmpresa, int idFactura);
    }
}
