using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Abstracciones.Interfaces.Reglas;
namespace Web.Servicios
{
    public sealed class SesionCookieEvents : CookieAuthenticationEvents
    {
        public const string NoDisponible="SesionTemporalmenteNoDisponible";
        private readonly IHttpClientFactory _clientes;
        private readonly IConfiguracion _configuracion;
        public SesionCookieEvents(IHttpClientFactory clientes,IConfiguracion configuracion){_clientes=clientes;_configuracion=configuracion;}
        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if(context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>()!=null) return;
            var token=context.Principal?.FindFirstValue("Token");
            if(string.IsNullOrWhiteSpace(token)){await Revocar(context);return;}
            try
            {
                var url=_configuracion.ObtenerValor("ApiEndPointsSeguridad:UrlBase");
                if(!Uri.TryCreate(url,UriKind.Absolute,out var uri) || uri.Scheme!="https" || !string.IsNullOrEmpty(uri.UserInfo)) throw new InvalidOperationException();
                using var solicitud=new HttpRequestMessage(HttpMethod.Get,uri.AbsoluteUri.TrimEnd('/')+"/Sesion");
                solicitud.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
                using var respuesta=await _clientes.CreateClient("SesionSeguridad").SendAsync(solicitud,context.HttpContext.RequestAborted);
                if(respuesta.StatusCode==HttpStatusCode.Unauthorized){await Revocar(context);return;}
                if(!respuesta.IsSuccessStatusCode)throw new HttpRequestException();
                var vigente=await respuesta.Content.ReadFromJsonAsync<SesionVigente>(cancellationToken:context.HttpContext.RequestAborted);
                if(vigente==null || vigente.IdUsuario<=0 || vigente.IdEmpresa<=0 || vigente.ExpiraUtc==null
                    || vigente.Roles==null || string.IsNullOrWhiteSpace(vigente.NombreUsuario) || string.IsNullOrWhiteSpace(vigente.Correo)) throw new InvalidOperationException();
                var plataforma=vigente.IdEmpresa==null;
                if(plataforma ? vigente.Roles.Count!=1 || vigente.Roles[0]!="Superadministrador" : vigente.Roles.Contains("Superadministrador"))throw new InvalidOperationException();
                if(vigente.ExpiraUtc<=DateTimeOffset.UtcNow
                    || vigente.IdUsuario.ToString()!=context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                    || (plataforma ? context.Principal?.FindFirstValue("Ambito")!="Plataforma" : vigente.IdEmpresa.ToString()!=context.Principal?.FindFirstValue("Empresa"))){await Revocar(context);return;}
                var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,vigente.IdUsuario.ToString()),
                    new(ClaimTypes.Name,vigente.NombreUsuario),new(ClaimTypes.Email,vigente.Correo),new("Token",token)};
                claims.Add(plataforma?new Claim("Ambito","Plataforma"):new Claim("Empresa",vigente.IdEmpresa!.Value.ToString()));
                claims.AddRange(vigente.Roles.Where(r=>!string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).Select(r=>new Claim(ClaimTypes.Role,r)));
                var anterior=context.Principal!.Claims.Select(c=>c.Type+"="+c.Value).Order().ToArray();
                var actual=claims.Select(c=>c.Type+"="+c.Value).Order().ToArray();
                context.ReplacePrincipal(new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)));
                if(context.Properties.ExpiresUtc==null || context.Properties.ExpiresUtc>vigente.ExpiraUtc) context.Properties.ExpiresUtc=vigente.ExpiraUtc;
                context.ShouldRenew=!anterior.SequenceEqual(actual);
                context.HttpContext.Response.Headers.CacheControl="no-store";
            }
            catch(Exception)
            {
                // Conservar la cookie cifrada para reintentar luego, pero no autorizar esta solicitud con claims viejos.
                context.RejectPrincipal();context.HttpContext.Items[NoDisponible]=true;
            }
        }
        private static async Task Revocar(CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        private sealed class SesionVigente
        {
            public int IdUsuario{get;set;} public int? IdEmpresa{get;set;}
            public string NombreUsuario{get;set;}="";public string Correo{get;set;}="";
            public List<string> Roles{get;set;}=new();public DateTimeOffset? ExpiraUtc{get;set;}
        }
    }
}