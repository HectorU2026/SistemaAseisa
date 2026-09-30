using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;

namespace Web.Views.Home.Inicio_Sesion
{
    public class Cambiar_ContrasenaModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public CambiarContrasenaRequest Cambio { get; set; } = default!;

        public Cambiar_ContrasenaModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Redirect("/Page/Seguridad/Inicio_Sesion");

            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (Cambio.NuevaContrasena != Cambio.ConfirmarContrasena)
                {
                    ModelState.AddModelError("", "Las contraseñas no coinciden");
                    return Page();
                }
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "CambiarContrasena");
                var client = new HttpClient();
                var Hash = Autenticacion.GenerarHash(Cambio.NuevaContrasena);
                var contrasena_hash = Autenticacion.ObtenerHash(Hash);
                var respuesta = await client.PostAsJsonAsync(endpoint,
                    new
                    {
                        Token = Cambio.Token,
                        NuevaContrasena = contrasena_hash
                    });

                if (!respuesta.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Error al cambiar la contraseña");
                    return Page();
                }
            }

            return Redirect("/Page/Seguridad/Inicio_Sesion");
        }
    }
}
