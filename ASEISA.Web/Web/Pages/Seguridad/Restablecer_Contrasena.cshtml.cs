using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Views.Home.Inicio_Sesion
{
    public class Restablecer_ContrasenaModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        public string? MensajeExito { get; set; }
        [BindProperty]
        public restablecerContraseña correoInfo { get; set; } = default!;

        public Restablecer_ContrasenaModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "RecuperarContrasena");
                var cliente = new HttpClient();
                var resultado = await cliente.PostAsJsonAsync<restablecerContraseña>(endpoint,
                    new restablecerContraseña
                    {
                        Correo = correoInfo.Correo
                    });
                MensajeExito = "Correo enviado con exito";
                if (resultado.IsSuccessStatusCode)
                    
                return Page();
            }

            return Page();
        }

    }

}
