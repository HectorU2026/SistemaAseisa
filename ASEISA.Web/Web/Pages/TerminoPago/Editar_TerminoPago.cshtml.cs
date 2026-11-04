using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.TerminoPago
{
    public class Editar_TerminoPagoModel : PageModel
    {
        private readonly TerminoPagoApiService _terminoPagoApiService;

        public Editar_TerminoPagoModel(
            TerminoPagoApiService terminoPagoApiService)
        {
            _terminoPagoApiService = terminoPagoApiService;
        }

        [BindProperty]
        public TerminoPagoRequest Termino { get; set; } = new();

        [BindProperty]
        public int IdTerminoPago { get; set; }

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int idTerminoPago)
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

            if (idTerminoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del término de pago no es válido.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }

            try
            {
                var terminos = await _terminoPagoApiService
                    .ObtenerTerminosPagoAsync(token);

                var termino = terminos.FirstOrDefault(t =>
                    t.IdTerminoPago == idTerminoPago);

                if (termino == null)
                {
                    TempData["MensajeError"] =
                        "El término de pago solicitado no existe.";

                    return RedirectToPage(
                        "/TerminoPago/Listado_TerminoPago");
                }

                IdTerminoPago = termino.IdTerminoPago;

                Termino = new TerminoPagoRequest
                {
                    Nombre = termino.Nombre,
                    DiasCredito = termino.DiasCredito
                };
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (InvalidOperationException)
            {
                TempData["MensajeError"] =
                    "No tiene permisos para consultar los términos de pago.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "No fue posible cargar el término de pago.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
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

            if (IdTerminoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del término de pago no es válido.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _terminoPagoApiService.EditarTerminoPagoAsync(
                    IdTerminoPago,
                    Termino,
                    token);

                TempData["MensajeExito"] =
                    "El término de pago se actualizó correctamente.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] =
                    "El término de pago solicitado no existe.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MensajeError =
                    "Ya existe otro término de pago con ese nombre.";
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
                    "No tiene permisos para editar términos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error al actualizar el término de pago.";
            }

            return Page();
        }
    }
}
