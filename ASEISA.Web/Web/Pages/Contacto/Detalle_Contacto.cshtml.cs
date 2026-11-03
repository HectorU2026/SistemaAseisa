using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Contacto
{
    public class Detalle_ContactoModel : PageModel
    {
        private readonly ContactoApiService _contactoApiService;

        public Detalle_ContactoModel(
            ContactoApiService contactoApiService)
        {
            _contactoApiService = contactoApiService;
        }

        public ContactoResponse? Contacto { get; private set; }

        public string? MensajeError { get; private set; }

        public async Task<IActionResult> OnGetAsync(int idContacto)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Seguridad/Login");

            string? token = User.FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToPage("/Seguridad/Login");

            if (idContacto <= 0)
                return RedirectToPage("/Contacto/Listado_Contacto");

            try
            {
                Contacto = await _contactoApiService
                    .ObtenerContactoPorIdAsync(idContacto, token);

                if (Contacto == null)
                    return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException ex)
            {
                MensajeError = ex.StatusCode switch
                {
                    HttpStatusCode.Forbidden =>
                        "No tiene permisos para consultar este contacto.",

                    _ =>
                        "No fue posible consultar el contacto. Verifique la conexión con la API."
                };
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error al consultar la información del contacto.";
            }

            return Page();
        }
    }
}
