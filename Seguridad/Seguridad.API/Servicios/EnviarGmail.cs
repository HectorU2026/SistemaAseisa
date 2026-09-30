using Abstracciones.DA;
using Abstracciones.Modelos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text;
using Abstracciones.Servicios;

namespace Servicios
{
    public class EnviarGmail : IEnviarGmail
    {
        private readonly IConfiguration _configuration;

        public EnviarGmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailUsuario(UsuarioSolicitado usuario, String enlace)
        {
            var remitenteCorreo = _configuration["Email:Remitente"];
            var remitenteNombre = _configuration["Email:Nombre"];
            var remitenteContrasena = _configuration["Email:Password"];

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("remitenteNombre", "remitenteCorreo"));
            msg.To.Add(new MailboxAddress("Usuario", usuario.correo));
            msg.Subject = "Restablecer Contraseña";

            var bb = new BodyBuilder
            {
                TextBody = $@"Hola {usuario.Nombre},

Recibimos una solicitud para restablecer tu contraseña.
Abre el siguiente enlace para continuar:

{enlace}

Si no solicitaste este cambio, ignora este correo.
El enlace expira en 15 minutos.",

                HtmlBody = $@"
                <h2>Restablecer contraseña</h2>
                <p>Hola <b>{usuario.Nombre}</b>,</p>
                <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                <p>
                    <a href='{enlace}' 
                       style='display:inline-block;padding:10px 20px;background:#0d6efd;
                              color:#fff;text-decoration:none;border-radius:5px;'>
                        Restablecer contraseña
                    </a>
                </p>
                <p>Si no solicitaste este cambio, ignora este correo.</p>
                <p><small>El enlace expira en 15 minutos.</small></p>"
            };
            msg.Body = bb.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(remitenteCorreo, remitenteContrasena);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}



