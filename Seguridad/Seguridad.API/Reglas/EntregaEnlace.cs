using Abstracciones.Reglas;
using Abstracciones.Modelos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Servicios;
using Servicios.Correo;
using System.Net;
namespace Reglas
{
    public sealed class EntregaEnlace:IEntregaEnlace
    {
        private readonly IConfiguration _configuracion;
        private readonly IConstructorCorreo _constructor;
        private readonly IEnviarCorreo _correo;
        private readonly ILogger<EntregaEnlace> _logger;
        public EntregaEnlace(IConfiguration configuracion,IConstructorCorreo constructor,IEnviarCorreo correo,ILogger<EntregaEnlace> logger)
        {_configuracion=configuracion;_constructor=constructor;_correo=correo;_logger=logger;}
        private Uri BaseEnlace()
        {
            if(!Uri.TryCreate(_configuracion["App:BaseUrl"],UriKind.Absolute,out var uri) || uri.Scheme!=Uri.UriSchemeHttps
                || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.Query))
                throw new InvalidOperationException("Configurar URL HTTPS de establecimiento de contrasena.");
            return uri;
        }
        public void ValidarConfiguracion()=>BaseEnlace();
        public async Task<bool> Enviar(DestinatarioContrasena destinatario,string token)
        {
            try
            {
                var builder=new UriBuilder(BaseEnlace()){Fragment="token="+Uri.EscapeDataString(token)};
                var contenido=await _constructor.RecuperarContrasena(WebUtility.HtmlEncode(destinatario.Nombre),builder.Uri.AbsoluteUri);
                await _correo.EnviarEmailUsuario(destinatario.Correo,contenido.Asunto,contenido.Html,contenido.TextoPlano);
                return true;
            }
            catch(Exception)
            {
                _logger.LogError(new EventId(1003,"EnlaceContrasenaNoEnviado"),"No se pudo enviar un enlace de establecimiento de contrasena.");
                return false;
            }
        }
    }
}