using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace Servicio
{
    public sealed class AdministracionCredenciales : IAdministracionCredenciales
    {
        private readonly IHttpClientFactory _clientes;
        private readonly IHttpContextAccessor _contexto;
        private readonly IConfiguration _configuracion;
        public AdministracionCredenciales(IHttpClientFactory clientes,IHttpContextAccessor contexto,IConfiguration configuracion)
        { _clientes=clientes;_contexto=contexto;_configuracion=configuracion; }
        public Task<ResultadoInvitacion> Registrar(UsuarioRequest solicitud,CancellationToken ct)=>Enviar("AdministracionUsuarios/Registrar",solicitud,ct);
        public Task<ResultadoInvitacion> Restablecer(int idUsuario,CancellationToken ct)=>Enviar($"AdministracionUsuarios/Restablecer/{idUsuario}",null,ct);
        private async Task<ResultadoInvitacion> Enviar(string ruta,object? cuerpo,CancellationToken ct)
        {
            // URL solo de configuracion administrada; nunca del navegador. TLS sin excepciones ni redirecciones.
            if(!Uri.TryCreate(_configuracion["Servicios:SeguridadUrl"],UriKind.Absolute,out var uri)
                || uri.Scheme!=Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                throw new HttpRequestException("Servicio no disponible.",null,System.Net.HttpStatusCode.ServiceUnavailable);
            var auth=_contexto.HttpContext?.Request.Headers.Authorization.ToString();
            if(!AuthenticationHeaderValue.TryParse(auth,out var bearer) || !string.Equals(bearer.Scheme,"Bearer",StringComparison.OrdinalIgnoreCase))
                throw new HttpRequestException("Sesion no valida.",null,System.Net.HttpStatusCode.Unauthorized);
            using var request=new HttpRequestMessage(HttpMethod.Post,new Uri(uri.AbsoluteUri.TrimEnd('/')+"/"+ruta));
            request.Headers.Authorization=bearer;
            if(cuerpo!=null) request.Content=JsonContent.Create(cuerpo);
            using var response=await _clientes.CreateClient("CredencialesSeguridad").SendAsync(request,ct);
            if(!response.IsSuccessStatusCode)
            {
                var codigo="OPERACION_RECHAZADA";
                try{
                    using var documento=System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
                    if(documento.RootElement.TryGetProperty("codigo",out var propiedad)
                        && propiedad.GetString() is string valor
                        && (valor is "CORREO_DUPLICADO" or "USUARIO_DUPLICADO" or "ROLES_INVALIDOS" or "DATOS_INVALIDOS" or "IDENTIFICACION_INVALIDA"))codigo=valor;
                }catch(System.Text.Json.JsonException){}
                throw new HttpRequestException(codigo,null,response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<ResultadoInvitacion>(cancellationToken:ct)
                ?? throw new HttpRequestException("Respuesta no disponible.",null,System.Net.HttpStatusCode.ServiceUnavailable);
        }
    }
}