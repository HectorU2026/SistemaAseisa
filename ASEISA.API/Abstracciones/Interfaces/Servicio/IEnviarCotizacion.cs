using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IEnviarCotizacion
    {
        Task<ActionResult> EnviarCotizacionPorGmail(int idCotizacion, string correoDestino);
    }
}
