using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.TerminoPago
{
    public class Agregar_TerminoPagoModel : PageModel
    {
        private readonly TerminoPagoApiService _terminoPagoApiService;

        public Agregar_TerminoPagoModel(
            TerminoPagoApiService terminoPagoApiService)
        {
            _terminoPagoApiService = terminoPagoApiService;
        }

        [BindProperty]
        public TerminoPagoRequest Termino { get; set; } = new();

        public string? MensajeError { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true ||
                string.IsNullOrWhiteSpace(User.FindFirst("Token")?.Value))
            {
                return RedirectToPage("/Seguridad/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _terminoPagoApiService.AgregarTerminoPagoAsync(
                    Termino,
                    token);

                TempData["MensajeExito"] =
                    "El término de pago se registró correctamente.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MensajeError =
                    "Ya existe un término de pago con ese nombre.";
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                MensajeError =
                    "Los datos ingresados no son válidos. Revise el formulario.";
            }
            catch (InvalidOperationException)
            {
                MensajeError =
                    "No tiene permisos para registrar términos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error al registrar el término de pago.";
            }

            return Page();
        }
    }
}
