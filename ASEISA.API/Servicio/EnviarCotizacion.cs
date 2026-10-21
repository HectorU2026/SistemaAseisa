using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Servicio;
using DA;
using Microsoft.AspNetCore.Mvc;
using Servicios;
using Servicios.Modelos;
using Servicios.Correo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Servicio
{
    public class EnviarCotizacion : IEnviarCotizacion
    {
        private ICotizacionDA _cotizacionDA;
        private readonly IEnviarCorreo _enviarCorreo;
        private readonly IConstructorCorreo _constructorCorreo;
        private readonly IGeneradorPdfCotizacion _generadorPdf;

        public EnviarCotizacion(ICotizacionDA cotizacionDA, IEnviarCorreo enviarCorreo, IConstructorCorreo constructorCorreo, IGeneradorPdfCotizacion generadorPdf)
        {
            _cotizacionDA = cotizacionDA;
            _enviarCorreo = enviarCorreo;
            _constructorCorreo = constructorCorreo;
            _generadorPdf = generadorPdf;
        }
        public async Task<ActionResult> EnviarCotizacionPorGmail(int idCotizacion, string correoDestino)
        {
            var cotizacion = await _cotizacionDA.ObtenerCotizacionPorId(idCotizacion);

            if (cotizacion == null)
                return new NotFoundObjectResult("Cotización no encontrada");

            if (!cotizacion.EstadoValido)
                return new BadRequestObjectResult(
                    $"La cotización debe estar en estado 'Cotización Enviada'. Actual: {cotizacion.NombreEstado}");

            if (string.IsNullOrWhiteSpace(correoDestino) || !EsCorreoValido(correoDestino))
                return new BadRequestObjectResult("Ingrese un correo electrónico válido");


            var pdf = _generadorPdf.Generar(cotizacion);

            var contenido = await _constructorCorreo.Cotizacion(cotizacion);

            var archivos = new[]
            {
                new Archivo
                {
                    Nombre = $"Cotizacion_{cotizacion.NumeroCotizacion}.pdf",
                    Contenido = pdf,
                    TipoMime = "application/pdf"
                }
            };

            await _enviarCorreo.EnviarEmailUsuario(
                correoDestino,
                contenido.Asunto,
                contenido.Html,
                contenido.TextoPlano,
                archivos);

            return new OkObjectResult("La cotización fue enviada exitosamente");
        }

        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }
    }
}
