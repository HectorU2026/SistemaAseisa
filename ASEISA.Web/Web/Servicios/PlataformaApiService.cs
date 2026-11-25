using ASEISA.Plataforma;
using Abstracciones.Interfaces.Reglas;
using System.Net;
using System.Net.Http.Headers;
namespace Web.Servicios;
public sealed class PlataformaApiService
{
    private readonly HttpClient http;private readonly IConfiguracion configuracion;
    public PlataformaApiService(HttpClient http,IConfiguracion configuracion){this.http=http;this.configuracion=configuracion;http.Timeout=TimeSpan.FromSeconds(20);}
    private async Task<HttpResponseMessage> Enviar(HttpMethod metodo,string ruta,string token,object? datos=null)
    {
        if(!Uri.TryCreate(configuracion.ObtenerValor("ApiEndPoints:UrlBase"),UriKind.Absolute,out var url)||url.Scheme!="https"||!string.IsNullOrEmpty(url.UserInfo))throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);
        using var request=new HttpRequestMessage(metodo,url.AbsoluteUri.TrimEnd('/')+"/Plataforma"+ruta);
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);if(datos!=null)request.Content=JsonContent.Create(datos);
        try{
            var response=await http.SendAsync(request);
            if(!response.IsSuccessStatusCode){var status=response.StatusCode;response.Dispose();throw new UsuariosApiException(status);}
            return response;
        }catch(HttpRequestException){throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}catch(TaskCanceledException){throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
    }
    public async Task<VistaPlataforma> Consultar(string token,int? empresa=null){using var r=await Enviar(HttpMethod.Get,empresa.HasValue?"?empresa="+empresa:"",token);return await r.Content.ReadFromJsonAsync<VistaPlataforma>()??throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
    public async Task<int> Guardar(string token,int? empresa,EmpresaPlataforma datos){using var r=await Enviar(empresa.HasValue?HttpMethod.Put:HttpMethod.Post,empresa.HasValue?"/empresas/"+empresa:"/empresas",token,datos);return await r.Content.ReadFromJsonAsync<int>();}
    public async Task EstadoEmpresa(string token,int empresa,bool activo){using var r=await Enviar(HttpMethod.Put,$"/empresas/{empresa}/estado",token,new{Activo=activo});}
    public async Task Registrar(string token,int empresa,PersonaPlataforma datos){using var r=await Enviar(HttpMethod.Post,$"/empresas/{empresa}/administradores",token,datos);}
    public async Task Editar(string token,int empresa,int usuario,EdicionAdministradorPlataforma datos){using var r=await Enviar(HttpMethod.Put,$"/empresas/{empresa}/administradores/{usuario}",token,datos);}
    public async Task Estado(string token,int empresa,int usuario,bool activo){using var r=await Enviar(HttpMethod.Put,$"/empresas/{empresa}/administradores/{usuario}/estado",token,new{Activo=activo});}
    public async Task Enlace(string token,int empresa,int usuario){using var r=await Enviar(HttpMethod.Post,$"/empresas/{empresa}/administradores/{usuario}/enlace",token);}
}