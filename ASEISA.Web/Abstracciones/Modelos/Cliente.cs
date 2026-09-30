using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Cliente
    {
        public int? IdCliente { get; set; }
        public int? IdEmpresa { get; set; }
        public string? TipoCliente { get; set; }
        public string? NombreRazonSocial { get; set; }
        [Required(ErrorMessage = "El campo Identificación es requerido")]
        public string Identificacion { get; set; }
        public string Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public decimal? LimiteCredito { get; set; }
        public int? DiasCredito { get; set; }
        public int? IdEstado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
