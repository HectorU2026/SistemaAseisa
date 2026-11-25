using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IReportesFlujo
    {
        Task<BalanceGeneralResponse> ObtenerBalanceGeneral(int idEmpresa, BalanceGeneralFiltro filtro);
        Task<LibroMayorResponse> ObtenerLibroMayor(int idEmpresa, LibroMayorRequest request);
        Task<ReporteCuentaPorPagarResponse> ObtenerReporteCuentaPorPagar(int idEmpresa, ReporteCuentaPorPagarRequest request);
    }
}
