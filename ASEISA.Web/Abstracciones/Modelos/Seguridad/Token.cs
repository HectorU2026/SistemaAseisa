using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos.Seguridad
{
    public class Token
    {
        public string? CodigoEstado { get; set; }
        public bool ValidacionExitosa { get; set; }
        public string? AccessToken { get; set; }
    }
}
