using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Servicios;
using System.Net.Mail;
using System.Text.Json;

namespace Servicio
{
    public class EnviarFactura : IEnviarFactura
    {
        private readonly IFacturaDA _facturaDA;
        private readonly IEmpresaDA _empresaDA;
        private readonly IBitacoraDA _bitacoraDA;
        private readonly IEnviarCorreo _enviarCorreo;
        private readonly IGeneradorPdfFactura _generadorPdf;

        public EnviarFactura(
            IFacturaDA facturaDA,
            IEmpresaDA empresaDA,
            IBitacoraDA bitacoraDA,
            IEnviarCorreo enviarCorreo,
            IGeneradorPdfFactura generadorPdf)
        {
            _facturaDA = facturaDA;
            _empresaDA = empresaDA;
            _bitacoraDA = bitacoraDA;
            _enviarCorreo = enviarCorreo;
            _generadorPdf = generadorPdf;
        }

        public async Task<ActionResult> Enviar(int idUsuario, int idEmpresa, int idFactura)
        {
            var factura = await _facturaDA.ObtenerFactura(idEmpresa, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            var rechazo = ValidarElegibilidad(factura);
            if (rechazo != null)
                return rechazo;

            var correoCliente = factura.CorreoCliente?.Trim();
            if (string.IsNullOrWhiteSpace(correoCliente) || !EsCorreoValido(correoCliente))
                return new BadRequestObjectResult("El cliente no tiene un correo electrónico válido");

            var enviosPrevios = await _facturaDA.ContarEnvios(idEmpresa, idFactura);
            var reenvio = enviosPrevios > 0;
            var empresa = await _empresaDA.ObtenerEmpresaPorId(idEmpresa);
            var pdf = _generadorPdf.Generar(factura, empresa);

            factura = await _facturaDA.ObtenerFactura(idEmpresa, idFactura);
            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            rechazo = ValidarElegibilidad(factura);
            if (rechazo != null)
                return rechazo;

            correoCliente = factura.CorreoCliente?.Trim();
            if (string.IsNullOrWhiteSpace(correoCliente) || !EsCorreoValido(correoCliente))
                return new BadRequestObjectResult("El cliente no tiene un correo electrónico válido");

            var asunto = reenvio
                ? $"Factura {factura.NumeroFactura} (reenvío)"
                : $"Factura {factura.NumeroFactura}";
            var nombreEmpresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "ASEISA";
            var html = ConstruirHtml(factura, nombreEmpresa, reenvio);
            var textoPlano = ConstruirTexto(factura, nombreEmpresa, reenvio);

            var archivos = new[]
            {
                new Archivo
                {
                    Nombre = $"Factura_{factura.NumeroFactura}.pdf",
                    Contenido = pdf,
                    TipoMime = "application/pdf"
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
            catch
            {
                return new ObjectResult("No se pudo enviar el correo de la factura")
                {
                    StatusCode = 502
                };
            }

            var valorNuevo = JsonSerializer.Serialize(new
            {
                numeroFactura = factura.NumeroFactura,
                correoCliente,
                reenvio,
                estadoFactura = factura.Estado
            });

            try
            {
                await _bitacoraDA.RegistrarBitacora(new Bitacora
                {
                    IdUsuario = idUsuario,
                    IdEmpresa = idEmpresa,
                    TablaAfectada = "FACTURA",
                    RegistroId = factura.IdFactura.ToString(),
                    Accion = "ENVIAR",
                    ValorAnterior = JsonSerializer.Serialize(new { enviosPrevios }),
                    ValorNuevo = valorNuevo
                });
            }
            catch
            {
                return new ObjectResult(new
                {
                    Mensaje = "La factura se envió al cliente, pero no se pudo registrar la bitácora.",
                    IdFactura = factura.IdFactura,
                    NumeroFactura = factura.NumeroFactura,
                    CorreoCliente = correoCliente,
                    Reenvio = reenvio,
                    BitacoraRegistrada = false
                })
                {
                    StatusCode = 500
                };
            }

            return new OkObjectResult(new
            {
                Mensaje = reenvio
                    ? "La factura fue reenviada exitosamente"
                    : "La factura fue enviada exitosamente",
                IdFactura = factura.IdFactura,
                NumeroFactura = factura.NumeroFactura,
                CorreoCliente = correoCliente,
                Reenvio = reenvio,
                Estado = factura.Estado,
                BitacoraRegistrada = true
            });
        }

        private static ActionResult? ValidarElegibilidad(FacturaDetalle factura)
        {
            if (string.Equals(factura.Estado, "Borrador", StringComparison.Ordinal))
                return new BadRequestObjectResult("La factura debe estar confirmada antes de enviarla");

            if (string.Equals(factura.Estado, "Inactiva", StringComparison.Ordinal))
                return new BadRequestObjectResult("No se puede enviar una factura inactiva");

            if (!string.Equals(factura.Estado, "Emitida", StringComparison.Ordinal)
                && !string.Equals(factura.Estado, "Pagada", StringComparison.Ordinal))
            {
                return new BadRequestObjectResult("La factura no se puede enviar en el estado actual");
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

        private static string ConstruirHtml(FacturaDetalle factura, string nombreEmpresa, bool reenvio)
        {
            var etiqueta = reenvio ? "reenvío" : "envío";
            return $@"<p>Estimado(a) {System.Net.WebUtility.HtmlEncode(factura.NombreCliente)},</p>
<p>Adjunto encontrará la factura <strong>{System.Net.WebUtility.HtmlEncode(factura.NumeroFactura)}</strong> ({etiqueta}) emitida el {factura.FechaEmision:dd/MM/yyyy} por un total de {factura.Total:N2} {System.Net.WebUtility.HtmlEncode(factura.Moneda ?? "")}.</p>
<p>{System.Net.WebUtility.HtmlEncode(nombreEmpresa)}</p>";
        }

        private static string ConstruirTexto(FacturaDetalle factura, string nombreEmpresa, bool reenvio)
        {
            var etiqueta = reenvio ? "reenvío" : "envío";
            return $"Estimado(a) {factura.NombreCliente},\r\n\r\nAdjunto encontrará la factura {factura.NumeroFactura} ({etiqueta}) emitida el {factura.FechaEmision:dd/MM/yyyy} por un total de {factura.Total:N2} {factura.Moneda}.\r\n\r\n{nombreEmpresa}";
        }
    }
}
