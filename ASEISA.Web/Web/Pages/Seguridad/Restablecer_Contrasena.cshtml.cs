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
            if (!ModelState.IsValid)
                return Page();

            try
            {
                string endpoint = _configuracion.ObtenerMetodo(
                    "ApiEndPointsSeguridad",
                    "RecuperarContrasena"
                );

                using var cliente = new HttpClient();

                var resultado = await cliente.PostAsJsonAsync(
                    endpoint,
                    new restablecerContraseña
                    {
                        Correo = correoInfo.Correo
                    }
                );

                if (resultado.IsSuccessStatusCode)
                {
                    MensajeExito = "Si el correo está asociado a una cuenta activa, recibirá un enlace con las instrucciones para restablecer su contraseña.";
                    return Page();
                }

                // No revelamos si el correo existe o no.
                if (resultado.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    MensajeExito = "Si el correo está asociado a una cuenta activa, recibirá un enlace con las instrucciones para restablecer su contraseña.";
                    return Page();
                }

                ModelState.AddModelError(
                    "",
                    "No fue posible procesar la solicitud en este momento. Inténtelo nuevamente más tarde."
                );
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "No fue posible conectar con el servicio. Inténtelo nuevamente más tarde."
                );
            }
            catch (TaskCanceledException)
            {
                ModelState.AddModelError(
                    "",
                    "La solicitud tardó demasiado en responder. Inténtelo nuevamente."
                );
            }

            return Page();
        }
    }
}
