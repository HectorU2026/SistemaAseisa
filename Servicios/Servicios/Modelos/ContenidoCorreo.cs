using System;
using System.Collections.Generic;
using System.Text;

namespace Servicios.Modelos
{
    public class ContenidoCorreo
    {
        public string Asunto { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public string TextoPlano { get; set; } = string.Empty;
    }
}
