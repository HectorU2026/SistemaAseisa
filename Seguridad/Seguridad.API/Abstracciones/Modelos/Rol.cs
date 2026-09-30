using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Modelos
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public int IdEstado { get; set; }

    }
}