using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.MetodoPago
{
    public class Agregar_MetodoPagoModel : PageModel
    {
        private readonly MetodoPagoApiService _metodoPagoApiService;

        public Agregar_MetodoPagoModel(
            MetodoPagoApiService metodoPagoApiService)
        {
            _metodoPagoApiService = metodoPagoApiService;
        }

        [BindProperty]
        public MetodoPagoRequest Metodo { get; set; } = new();

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
                await _metodoPagoApiService.AgregarMetodoPagoAsync(
                    Metodo,
                    token);

                TempData["MensajeExito"] =
                    "El método de pago se registró correctamente.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MensajeError =
                    "Ya existe un método de pago con ese nombre.";
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
                    "No tiene permisos para registrar métodos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error al registrar el método de pago.";
            }

            return Page();
        }
    }
}
