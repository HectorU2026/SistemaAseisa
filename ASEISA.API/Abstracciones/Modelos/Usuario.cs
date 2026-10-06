using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Usuario
    {
        public int? IdUsuario { get; set; }
        public int? IdEmpresa { get; set; }
        public string? TipoCliente { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string Correo { get; set; }
        public string NombreUsuario { get; set; }
        public string ContrasenaHash { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int? IdEstado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

}
