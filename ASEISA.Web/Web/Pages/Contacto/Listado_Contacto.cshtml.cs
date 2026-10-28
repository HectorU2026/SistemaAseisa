using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Servicios;
using System.Net;

namespace Web.Pages.Contacto
{
    public class Listado_ContactoModel : PageModel
    {
        private readonly ContactoApiService _contactoApiService;

        public Listado_ContactoModel(
            ContactoApiService contactoApiService)
        {
            _contactoApiService = contactoApiService;
        }

        public List<ContactoResponse> Contactos { get; set; } = new();

        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RolContacto { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Estado { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Seguridad/Login");
            }

            string? token = User.FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Seguridad/Login");
            }

            try
            {
                var contactos = await _contactoApiService
                    .ObtenerContactosAsync(token);

                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    string busqueda = Busqueda.Trim();

                    contactos = contactos.Where(c =>
                        c.Nombre.Contains(busqueda,
                            StringComparison.OrdinalIgnoreCase) ||
                        (c.Cedula?.Contains(busqueda,
                            StringComparison.OrdinalIgnoreCase) ?? false) ||
                        c.Correo.Contains(busqueda,
                            StringComparison.OrdinalIgnoreCase) ||
                        c.Telefono.Contains(busqueda,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (!string.IsNullOrWhiteSpace(RolContacto))
                {
                    contactos = contactos.Where(c =>
                        string.Equals(
                            c.RolContacto,
                            RolContacto,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (!string.IsNullOrWhiteSpace(Estado))
                {
                    contactos = contactos.Where(c =>
                        string.Equals(
                            c.Estado,
                            Estado,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                Contactos = contactos
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (Exception)
            {
                MensajeError =
                    "No fue posible cargar los contactos. Intente nuevamente.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostActivarAsync(
            int idContacto)
        {
            return await CambiarEstadoAsync(
                idContacto,
                activar: true);
        }

        public async Task<IActionResult> OnPostDesactivarAsync(
            int idContacto)
        {
            return await CambiarEstadoAsync(
                idContacto,
                activar: false);
        }

        private async Task<IActionResult> CambiarEstadoAsync(
            int idContacto,
            bool activar)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Seguridad/Login");
            }

            string? token = User.FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToPage("/Seguridad/Login");
            }

            if (idContacto <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del contacto no es válido.";

                return RedirectToPage("/Contacto/Listado_Contacto");
            }

            try
            {
                if (activar)
                {
                    await _contactoApiService
                        .ActivarContactoAsync(idContacto, token);

                    TempData["MensajeExito"] =
                        "El contacto se activó correctamente.";
                }
                else
                {
                    await _contactoApiService
                        .DesactivarContactoAsync(idContacto, token);

                    TempData["MensajeExito"] =
                        "El contacto se desactivó correctamente.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] =
                    "El contacto solicitado no existe.";
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["MensajeError"] =
                    "No fue posible cambiar el estado del contacto debido a un conflicto.";
            }
            catch (InvalidOperationException)
            {
                TempData["MensajeError"] =
                    "No tiene permisos para realizar esta operación.";
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "Ocurrió un error al cambiar el estado del contacto.";
            }

            return RedirectToPage("/Contacto/Listado_Contacto");
        }
    }
}
