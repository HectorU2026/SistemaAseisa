using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Flujo
{
    public class ReportesFlujo : IReportesFlujo
    {
        private IReportesDA _reportesDA;

        public ReportesFlujo(IReportesDA reportesDA)
        {
            _reportesDA = reportesDA;
        }

        public async Task<BalanceGeneralResponse> ObtenerBalanceGeneral(int idEmpresa, BalanceGeneralFiltro filtro)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarFechas(filtro.FechaCorte1, filtro.FechaCorte2);new BalanceGeneralResponse();
            
            return await _reportesDA.ObtenerBalanceGeneral(idEmpresa, filtro);
        }

        public async Task<LibroMayorResponse> ObtenerLibroMayor(int idEmpresa, LibroMayorRequest request)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarFechas(request.FechaInicio, request.FechaFin);

            return await _reportesDA.ObtenerLibroMayor(idEmpresa, request);
        }

        public async Task<ReporteCuentaPorPagarResponse> ObtenerReporteCuentaPorPagar(int idEmpresa, ReporteCuentaPorPagarRequest request)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));    
            ValidarFechaCorte(request.FechaCorte);
            return await _reportesDA.ObtenerReporteCuentaPorPagar(idEmpresa, request);
        }


        #region "Helpers"

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static void ValidarId(int valor, string nombre)
        {
            if (valor <= 0)
                throw new ValidationException($"{nombre} debe ser mayor que cero.");
        }


        private static void ValidarFechas(DateTime fecha1, DateTime? fecha2)
        {
            if (fecha1 == default(DateTime))
                throw new ValidationException("La fecha de corte es obligatoria");

            if (fecha1.Date > DateTime.Today)
                throw new ValidationException("La fecha de corte no puede ser posterior a la fecha actual");

            if (fecha2.HasValue)
            {
                if (fecha2.Value == default(DateTime))
                    throw new ValidationException("La fecha del periodo comparativo no es válida");

                if (fecha2.Value.Date > DateTime.Today)
                    throw new ValidationException("La fecha del periodo comparativo no puede ser posterior a la fecha actual");

                if (fecha2.Value.Date >= fecha1.Date)
                    throw new ValidationException("La fecha del periodo comparativo debe ser anterior a la fecha de corte principal");
            }
        }

        private static void ValidarFechaCorte(DateTime? fecha)
        {
            if (fecha.HasValue)
            {
                if (fecha.Value == default(DateTime))
                    throw new ValidationException("La fecha de corte no es válida.");

                if (fecha.Value.Date > DateTime.Today)
                    throw new ValidationException("La fecha de corte no puede ser posterior a la fecha actual.");
            }
        }

        #endregion
    }
}
