using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.MetodoPago
{
    public class Editar_MetodoPagoModel : PageModel
    {
        private readonly MetodoPagoApiService _metodoPagoApiService;

        public Editar_MetodoPagoModel(
            MetodoPagoApiService metodoPagoApiService)
        {
            _metodoPagoApiService = metodoPagoApiService;
        }

        [BindProperty]
        public MetodoPagoRequest Metodo { get; set; } = new();

        [BindProperty]
        public int IdMetodoPago { get; set; }

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int idMetodoPago)
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

            if (idMetodoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del método de pago no es válido.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }

            try
            {
                var metodos = await _metodoPagoApiService
                    .ObtenerMetodosPagoAsync(token);

                var metodo = metodos.FirstOrDefault(m =>
                    m.IdMetodoPago == idMetodoPago);

                if (metodo == null)
                {
                    TempData["MensajeError"] =
                        "El método de pago solicitado no existe.";

                    return RedirectToPage(
                        "/MetodoPago/Listado_MetodoPago");
                }

                IdMetodoPago = metodo.IdMetodoPago;

                Metodo = new MetodoPagoRequest
                {
                    Nombre = metodo.Nombre
                };
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (InvalidOperationException)
            {
                TempData["MensajeError"] =
                    "No tiene permisos para consultar los métodos de pago.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el método de pago.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
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

            if (IdMetodoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del método de pago no es válido.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _metodoPagoApiService.EditarMetodoPagoAsync(
                    IdMetodoPago,
                    Metodo,
                    token);

                TempData["MensajeExito"] =
                    "El método de pago se actualizó correctamente.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] =
                    "El método de pago solicitado no existe.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MensajeError =
                    "Ya existe otro método de pago con ese nombre.";
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
                    "No tiene permisos para editar métodos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error al actualizar el método de pago.";
            }

            return Page();
        }
    }
}
