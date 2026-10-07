using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Empresa
    {
        public int? IdEmpresa { get; set; }

        [Required]
        [StringLength(200)]
        public string RazonSocial { get; set; }

        [StringLength(200)]
        public string? NombreComercial { get; set; }

        [Required]
        [StringLength(30)]
        public string CedulaJuridica { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Correo { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [Required]
        [StringLength(10)]
        public string MonedaBase { get; set; }

        public int? IdEstado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class EmpresaResponseNombres 
    {
        public int IdEmpresa { get; set; }
        public string? NombreComercial { get; set; }
    }
}
