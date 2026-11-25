using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Web.Views.Home.Inicio_Sesion
{
    public class Cambiar_ContrasenaModel:PageModel
    {
        private readonly IConfiguracion _configuracion;
        private readonly IHttpClientFactory _clientes;
        public string Token{get;set;}="";
        [BindProperty]public CambiarContrasenaRequest Cambio{get;set;}=new();
        public Cambiar_ContrasenaModel(IConfiguracion configuracion,IHttpClientFactory clientes){_configuracion=configuracion;_clientes=clientes;}
        private void Preparar(string? token)
        {
            Response.Headers["Referrer-Policy"]="no-referrer";Response.Headers.CacheControl="no-store";Token=token??"";
        }
        public IActionResult OnGet(string? token){Preparar(token);return Page();}
        public async Task<IActionResult> OnPostAsync()
        {
            Preparar(Cambio.Token);
            if(!ModelState.IsValid)return Page();
            try
            {
                var url=_configuracion.ObtenerMetodo("ApiEndPointsSeguridad","CambiarContrasena");
                if(!Uri.TryCreate(url,UriKind.Absolute,out var uri) || uri.Scheme!="https" || !string.IsNullOrEmpty(uri.UserInfo)) throw new HttpRequestException();
                using var respuesta=await _clientes.CreateClient("SesionSeguridad").PostAsJsonAsync(uri,new{Token=Cambio.Token,ContrasenaHash=Cambio.NuevaContrasena});
                if(respuesta.IsSuccessStatusCode)return RedirectToPage("/Seguridad/Login");
                ModelState.AddModelError("",respuesta.StatusCode==System.Net.HttpStatusCode.BadRequest
                    ? "El enlace no es valido, vencio o ya fue utilizado. Solicite uno nuevo."
                    : "No fue posible confirmar el cambio. Intente iniciar sesion antes de solicitar otro enlace.");
            }
            catch(HttpRequestException){ModelState.AddModelError("","No fue posible confirmar el cambio. Verifique su acceso antes de solicitar otro enlace.");}
            catch(TaskCanceledException){ModelState.AddModelError("","La solicitud demoro demasiado. Verifique su acceso antes de solicitar otro enlace.");}
            return Page();
        }
    }
}