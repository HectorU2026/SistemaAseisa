using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Bitacora
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string TablaAfectada { get; set; }
        public string? RegistroId { get; set; }
        public string Accion { get; set; }
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
    }
}
