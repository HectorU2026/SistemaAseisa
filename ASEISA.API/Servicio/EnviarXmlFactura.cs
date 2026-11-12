using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Servicios;
using System.Net.Mail;
using System.Text.Json;

namespace Servicio
{
    public class EnviarXmlFactura : IEnviarXmlFactura
    {
        private const string MensajeErrorXml = "No fue posible generar o enviar el archivo XML";
        private const string AvisoNoTributario =
            "El archivo XML adjunto es un documento académico interno de ASEISA. No constituye un comprobante electrónico válido ante Hacienda ni cumple la versión 4.4 de factura electrónica.";

        private readonly IFacturaDA _facturaDA;
        private readonly IEmpresaDA _empresaDA;
        private readonly IBitacoraDA _bitacoraDA;
        private readonly IEnviarCorreo _enviarCorreo;
        private readonly IGeneradorXmlFactura _generadorXml;

        public EnviarXmlFactura(
            IFacturaDA facturaDA,
            IEmpresaDA empresaDA,
            IBitacoraDA bitacoraDA,
            IEnviarCorreo enviarCorreo,
            IGeneradorXmlFactura generadorXml)
        {
            _facturaDA = facturaDA;
            _empresaDA = empresaDA;
            _bitacoraDA = bitacoraDA;
            _enviarCorreo = enviarCorreo;
            _generadorXml = generadorXml;
        }

        public async Task<ActionResult> Enviar(int idUsuario, int idEmpresa, int idFactura)
        {
            var factura = await _facturaDA.ObtenerFactura(idEmpresa, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            var rechazo = ValidarElegibilidadXml(factura);
            if (rechazo != null)
                return rechazo;

            var correoCliente = factura.CorreoCliente?.Trim();
            if (string.IsNullOrWhiteSpace(correoCliente) || !EsCorreoValido(correoCliente))
            {
                await RegistrarErrorXml(idUsuario, idEmpresa, factura.IdFactura, "correo");
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            var empresa = await _empresaDA.ObtenerEmpresaPorId(idEmpresa);
            byte[] xml;

            try
            {
                xml = _generadorXml.Generar(factura, empresa);
            }
            catch (Exception ex)
            {
                await RegistrarErrorXml(idUsuario, idEmpresa, factura.IdFactura, "generacion", ex);
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            var nombreEmpresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "ASEISA";
            var asunto = $"XML interno factura {factura.NumeroFactura}";
            var html = ConstruirHtml(factura, nombreEmpresa);
            var textoPlano = ConstruirTexto(factura, nombreEmpresa);
            var archivos = new[]
            {
                new Archivo
                {
                    Nombre = NombreArchivoXml(factura.NumeroFactura),
                    Contenido = xml,
                    TipoMime = "application/xml"
                }
            };

            try
            {
                await _enviarCorreo.EnviarEmailUsuario(
                    correoCliente,
                    asunto,
                    html,
                    textoPlano,
                    archivos);
            }
            catch (Exception ex)
            {
                await RegistrarErrorXml(idUsuario, idEmpresa, factura.IdFactura, "smtp", ex);
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            try
            {
                await _bitacoraDA.RegistrarBitacora(new Bitacora
                {
                    IdUsuario = idUsuario,
                    IdEmpresa = idEmpresa,
                    TablaAfectada = "FACTURA",
                    RegistroId = factura.IdFactura.ToString(),
                    Accion = "ENVIAR_XML",
                    ValorNuevo = JsonSerializer.Serialize(new
                    {
                        numeroFactura = factura.NumeroFactura,
                        correoCliente,
                        estadoFactura = factura.Estado
                    })
                });
            }
            catch
            {
                return new ObjectResult(new
                {
                    Mensaje = "El XML se envió al cliente, pero no se pudo registrar la bitácora.",
                    IdFactura = factura.IdFactura,
                    NumeroFactura = factura.NumeroFactura,
                    CorreoCliente = correoCliente,
                    BitacoraRegistrada = false
                })
                {
                    StatusCode = 500
                };
            }

            return new OkObjectResult(new
            {
                Mensaje = "El XML interno de la factura fue enviado exitosamente",
                IdFactura = factura.IdFactura,
                NumeroFactura = factura.NumeroFactura,
                CorreoCliente = correoCliente,
                Estado = factura.Estado,
                BitacoraRegistrada = true
            });
        }

        private static ActionResult? ValidarElegibilidadXml(FacturaDetalle factura)
        {
            if (string.Equals(factura.Estado, "Borrador", StringComparison.Ordinal))
                return new BadRequestObjectResult("La factura debe estar confirmada antes de generar el XML");

            if (string.Equals(factura.Estado, "Inactiva", StringComparison.Ordinal))
                return new BadRequestObjectResult("No se puede generar el XML de una factura inactiva");

            if (!string.Equals(factura.Estado, "Emitida", StringComparison.Ordinal)
                && !string.Equals(factura.Estado, "Pagada", StringComparison.Ordinal))
            {
                return new BadRequestObjectResult(MensajeErrorXml);
            }

            return null;
        }

        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        private static string NombreArchivoXml(string? numeroFactura)
        {
            var numero = string.IsNullOrWhiteSpace(numeroFactura) ? "sin-numero" : numeroFactura.Trim();
            var invalidos = Path.GetInvalidFileNameChars();
            var sanitizado = new string(numero.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
            return $"Factura_{sanitizado}.xml";
        }

        private static string ConstruirHtml(FacturaDetalle factura, string nombreEmpresa)
        {
            return $@"<p>Estimado(a) {System.Net.WebUtility.HtmlEncode(factura.NombreCliente)},</p>
<p>Adjunto encontrará el XML interno de la factura <strong>{System.Net.WebUtility.HtmlEncode(factura.NumeroFactura)}</strong>.</p>
<p><strong>{System.Net.WebUtility.HtmlEncode(AvisoNoTributario)}</strong></p>
<p>{System.Net.WebUtility.HtmlEncode(nombreEmpresa)}</p>";
        }

        private static string ConstruirTexto(FacturaDetalle factura, string nombreEmpresa)
        {
            return $"Estimado(a) {factura.NombreCliente},\r\n\r\nAdjunto encontrará el XML interno de la factura {factura.NumeroFactura}.\r\n\r\n{AvisoNoTributario}\r\n\r\n{nombreEmpresa}";
        }

        private async Task RegistrarErrorXml(
            int idUsuario,
            int idEmpresa,
            int idFactura,
            string etapa,
            Exception? ex = null)
        {
            try
            {
                object payload = ex == null
                    ? new { etapa }
                    : new { etapa, tipo = ex.GetType().Name };

                await _bitacoraDA.RegistrarBitacora(new Bitacora
                {
                    IdUsuario = idUsuario,
                    IdEmpresa = idEmpresa,
                    TablaAfectada = "FACTURA",
                    RegistroId = idFactura.ToString(),
                    Accion = "ERROR_XML",
                    ValorNuevo = JsonSerializer.Serialize(payload)
                });
            }
            catch
            {
            }
        }
    }
}
