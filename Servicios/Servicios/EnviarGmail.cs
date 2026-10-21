using Abstracciones.Modelos;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Servicios
{
    public class Archivo
    {
        public string Nombre { get; set; } = string.Empty;
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public string TipoMime { get; set; } = "application/octet-stream";
    }

    public interface IEnviarCorreo
    {
        Task EnviarEmailUsuario(string destinatario, string asunto, string html, string textoPlano, IEnumerable<Archivo>? archivo = null);
    }
    public class EnviarCorreo : IEnviarCorreo
    {
        private readonly IConfiguration _configuration;

        public EnviarCorreo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailUsuario(string destinatario, string asunto, string html, string textoPlano, IEnumerable<Archivo>? archivo = null)
        {
            var remitenteCorreo = _configuration["Email:Remitente"];
            var remitenteNombre = _configuration["Email:Nombre"];
            var remitenteContrasena = _configuration["Email:Password"];

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(remitenteNombre, remitenteCorreo));
            msg.To.Add(new MailboxAddress("", destinatario));
            msg.Subject = asunto;

            var bb = new BodyBuilder
            {

                TextBody = textoPlano,
                HtmlBody = html
            };

            if (archivo != null)
            {
                foreach (var adjunto in archivo)
                {
                    bb.Attachments.Add(
                        adjunto.Nombre,
                        adjunto.Contenido,
                        ContentType.Parse(adjunto.TipoMime));
                }
            }


            msg.Body = bb.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(remitenteCorreo, remitenteContrasena);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}