using Abstracciones.Interfaces.Reglas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;
using System.Text;
using Web.Servicios;
internal static class PruebasCookies
{
    public static async Task<int> Ejecutar()
    {
        var casos=0;
        async Task Caso(HttpStatusCode estado,string contenido,bool revocada,bool temporal,string[] roles)
        {
            var servicios=new ServiceCollection();servicios.AddLogging();servicios.AddAuthentication().AddCookie();
            await using var proveedor=servicios.BuildServiceProvider();
            var http=new DefaultHttpContext{RequestServices=proveedor};
            var principal=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,"7"),new Claim("Empresa","8"),new Claim(ClaimTypes.Role,"RolAntiguo"),new Claim("Token","token-sintetico")},"Cookies"));
            var props=new AuthenticationProperties{ExpiresUtc=DateTimeOffset.UtcNow.AddMinutes(1)};var limite=props.ExpiresUtc;
            var contexto=new CookieValidatePrincipalContext(http,new AuthenticationScheme("Cookies",null,typeof(CookieAuthenticationHandler)),new CookieAuthenticationOptions(),new AuthenticationTicket(principal,props,"Cookies"));
            var eventos=new SesionCookieEvents(new ClientesFake(estado,contenido),new ConfigFake());
            await eventos.ValidatePrincipal(contexto);
            if((contexto.Principal==null)!=(revocada||temporal) || http.Items.ContainsKey(SesionCookieEvents.NoDisponible)!=temporal)throw new Exception("Estado de cookie incorrecto");
            if(!revocada&&!temporal && (roles.Any(r=>!contexto.Principal!.IsInRole(r)) || contexto.Principal!.IsInRole("RolAntiguo") || contexto.Properties.ExpiresUtc>limite))throw new Exception("Claims o vigencia incorrectos");
            if(temporal && http.Response.Headers.SetCookie.Count!=0)throw new Exception("Indisponibilidad elimino cookie");
            Console.WriteLine("OK Cookie "+estado+" revocada="+revocada+" temporal="+temporal);casos++;
        }
        var fecha=DateTimeOffset.UtcNow.AddMinutes(5).ToString("O");
        await Caso(HttpStatusCode.OK,"{\"idUsuario\":7,\"idEmpresa\":8,\"nombreUsuario\":\"actual\",\"correo\":\"actual@example.invalid\",\"roles\":[\"Administrador\",\"Cliente\"],\"expiraUtc\":\""+fecha+"\"}",false,false,["Administrador","Cliente"]);
        await Caso(HttpStatusCode.OK,"{\"idUsuario\":7,\"idEmpresa\":8,\"nombreUsuario\":\"actual\",\"correo\":\"actual@example.invalid\",\"roles\":[],\"expiraUtc\":\""+fecha+"\"}",false,false,[]);
        await Caso(HttpStatusCode.Unauthorized,"",true,false,[]);
        await Caso(HttpStatusCode.ServiceUnavailable,"",false,true,[]);
        await Caso(HttpStatusCode.OK,"{}",false,true,[]);
        return casos;
    }
    private sealed class ConfigFake:IConfiguracion
    {public string ObtenerValor(string k)=>"https://seguridad.example.invalid/api";public string ObtenerMetodo(string s,string n)=>throw new NotSupportedException();}
    private sealed class ClientesFake:HttpMessageHandler,IHttpClientFactory
    {
        private readonly HttpStatusCode _estado;private readonly string _contenido;
        public ClientesFake(HttpStatusCode estado,string contenido){_estado=estado;_contenido=contenido;}
        public HttpClient CreateClient(string n)=>new(this,false);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)=>Task.FromResult(new HttpResponseMessage(_estado){Content=new StringContent(_contenido,Encoding.UTF8,"application/json")});
    }
}