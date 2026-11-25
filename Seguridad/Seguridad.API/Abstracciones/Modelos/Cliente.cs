using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Modelos
{
    public class Cliente
    {
        public int? IdCliente { get; set; }
        public int? IdEmpresa { get; set; }
        [StringLength(50)]
        public string TipoCliente { get; set; }
        [StringLength(200)]
        public string NombreRazonSocial { get; set; }
        [StringLength(50)]
        public string Identificacion { get; set; }
        public string? Correo { get; set; }
        [ASEISA.Usuarios.TelefonoUsuario]
        public string? Telefono { get; set; }
        [StringLength(250,ErrorMessage="La dirección no puede superar los 250 caracteres.")]
        public string? Direccion { get; set; }
        public decimal? LimiteCredito { get; set; }
        public int? DiasCredito { get; set; }
        public int? IdEstado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class RegistroRequest
    {
        public Usuario Usuario { get; set; }
        public Cliente Cliente { get; set; }
    }

    public class RegistroResponse
    {
        public int? IdUsuario { get; set; }
        public string mensaje { get; set; }
    }
}
