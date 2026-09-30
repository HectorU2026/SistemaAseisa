using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos.Servicios;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;

namespace Reglas
{
    public class Configuracion : IConfiguracion
    {
        private IConfiguration _configuration;

        public Configuracion (IConfiguration configuration)
        { 
            _configuration = configuration;
        }

        
        public string ObtenerMetodo(string seccion, string nombre) 
        {
            string? UrlBase = ObtenerUrlBase(seccion);
            var Metodo = _configuration.GetSection(seccion).Get<APIEndPoint>().Metodos.Where(m => m.Nombre == nombre).FirstOrDefault().Valor;
            return $"{UrlBase}/{Metodo}";
        }

        public string ObtenerValor(string llave)
        {
            return _configuration.GetSection(llave).Value;
        }

        public string ObtenerUrlBase(string seccion)
        {
            return _configuration.GetSection(seccion).Get<APIEndPoint>().UrlBase;
        }


    }
}
