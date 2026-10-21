using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class ProductoCatalogoRespuesta
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int Total { get; set; }
        public IEnumerable<ProductoCatalogoItem> Items { get; set; }
    }
}
