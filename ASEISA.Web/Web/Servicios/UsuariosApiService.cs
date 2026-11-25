using Abstracciones.Interfaces.Reglas;
using Web.Modelos;
using System.Net;
using System.Net.Http.Headers;
namespace Web.Servicios
{
    public sealed class UsuariosApiException:Exception
    {
        public HttpStatusCode Estado{get;}
        public string? Codigo{get;}
        public UsuariosApiException(HttpStatusCode estado,string? codigo=null){Estado=estado;Codigo=codigo;}
    }
    public sealed class UsuariosApiService
    {
        private readonly HttpClient _http;private readonly IConfiguracion _configuracion;
        public UsuariosApiService(HttpClient http,IConfiguracion configuracion){_http=http;_configuracion=configuracion;_http.Timeout=TimeSpan.FromSeconds(20);}
        private async Task<HttpResponseMessage> Enviar(HttpMethod metodo,string ruta,string token,object? cuerpo=null)
        {
            var url=_configuracion.ObtenerValor("ApiEndPoints:UrlBase");
            if(!Uri.TryCreate(url,UriKind.Absolute,out var uri) || uri.Scheme!="https" || !string.IsNullOrEmpty(uri.UserInfo))throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);
            using var request=new HttpRequestMessage(metodo,uri.AbsoluteUri.TrimEnd('/')+"/Usuario/"+ruta);
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
            if(cuerpo!=null)request.Content=JsonContent.Create(cuerpo);
            try
            {
                var response=await _http.SendAsync(request);
                if(!response.IsSuccessStatusCode)
                {
                    var estado=response.StatusCode;string? codigo=null;
                    try{using var documento=System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        if(documento.RootElement.TryGetProperty("codigo",out var propiedad))codigo=propiedad.GetString();}
                    catch(System.Text.Json.JsonException){}
                    finally{response.Dispose();}
                    throw new UsuariosApiException(estado,codigo);
                }
                return response;
            }
            catch(HttpRequestException){throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
            catch(TaskCanceledException){throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
        }
        public async Task<List<UsuarioAdministrado>> Listar(string token,bool pendientes=false)
        {using var r=await Enviar(HttpMethod.Get,pendientes?"ObtenerSolicitudesClientesPendientes":"ObtenerUsuarios",token);return await r.Content.ReadFromJsonAsync<List<UsuarioAdministrado>>()??new();}
        public async Task<UsuarioAdministrado> Obtener(int id,string token)
        {using var r=await Enviar(HttpMethod.Get,$"ObtenerUsuarioPorId/{id}",token);return await r.Content.ReadFromJsonAsync<UsuarioAdministrado>()??throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
        public async Task<CatalogosUsuarios> Catalogos(string token)
        {using var r=await Enviar(HttpMethod.Get,"Catalogos",token);return await r.Content.ReadFromJsonAsync<CatalogosUsuarios>()??throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
        public async Task<ResultadoInvitacion> Registrar(FormularioUsuario datos,string token)
        {using var r=await Enviar(HttpMethod.Post,"AgregarUsuario",token,datos);return new(){IdUsuario=await r.Content.ReadFromJsonAsync<int>(),EnvioProgramado=r.Headers.TryGetValues("X-Invitacion-Estado",out var valores)&&valores.Contains("Programado")};}
        public async Task Editar(int id,DatosPersonalesUsuario datos,string token){using var r=await Enviar(HttpMethod.Put,$"EditarUsuario/{id}",token,datos);}
        public async Task Estado(int id,bool activo,string token){using var r=await Enviar(HttpMethod.Put,$"CambiarEstadoUsuario/{id}",token,new{Activo=activo});}
        public async Task Aprobar(int id,bool aprobar,string token){using var r=await Enviar(HttpMethod.Put,$"AprobarRegistroCliente/{id}",token,new{Aprobar=aprobar});}
        public async Task Rol(int id,int rol,bool agregar,string token)
        {using var r=await Enviar(agregar?HttpMethod.Post:HttpMethod.Delete,agregar?$"AsignarRolUsuario/{id}":$"RetirarRolUsuario/{id}/roles/{rol}",token,agregar?new{IdRol=rol}:null);}
        public async Task<ResultadoInvitacion> Restablecer(int id,string token)
        {using var r=await Enviar(HttpMethod.Post,$"RestablecerContrasenaUsuario/{id}",token);return await r.Content.ReadFromJsonAsync<ResultadoInvitacion>()??throw new UsuariosApiException(HttpStatusCode.ServiceUnavailable);}
    }
}