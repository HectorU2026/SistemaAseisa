using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.TerminoPago
{
    public class Listado_TerminoPagoModel : PageModel
    {
        private readonly TerminoPagoApiService _terminoPagoApiService;

        public Listado_TerminoPagoModel(
            TerminoPagoApiService terminoPagoApiService)
        {
            _terminoPagoApiService = terminoPagoApiService;
        }

        public List<TerminoPagoResponse> TerminosPago { get; set; } = new();

        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

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
                var terminos = await _terminoPagoApiService
                    .ObtenerTerminosPagoAsync(token);

                // Filtrar por nombre
                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    string busqueda = Busqueda.Trim();

                    terminos = terminos.Where(t =>
                        t.Nombre.Contains(
                            busqueda,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Filtrar por estado
                if (!string.IsNullOrWhiteSpace(Estado))
                {
                    if (Estado.Equals(
                        "Activo",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        terminos = terminos.Where(t => t.Activo).ToList();
                    }
                    else if (Estado.Equals(
                        "Inactivo",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        terminos = terminos.Where(t => !t.Activo).ToList();
                    }
                }

                TerminosPago = terminos
                    .OrderBy(t => t.DiasCredito)
                    .ThenBy(t => t.Nombre)
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (InvalidOperationException)
            {
                MensajeError =
                    "No tiene permisos para consultar los términos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "No fue posible cargar los términos de pago. Intente nuevamente.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostActivarAsync(
            int idTerminoPago)
        {
            return await CambiarEstadoAsync(
                idTerminoPago,
                activar: true);
        }

        public async Task<IActionResult> OnPostDesactivarAsync(
            int idTerminoPago)
        {
            return await CambiarEstadoAsync(
                idTerminoPago,
                activar: false);
        }

        private async Task<IActionResult> CambiarEstadoAsync(
            int idTerminoPago,
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

            if (idTerminoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del término de pago no es válido.";

                return RedirectToPage(
                    "/TerminoPago/Listado_TerminoPago");
            }

            try
            {
                if (activar)
                {
                    await _terminoPagoApiService
                        .ActivarTerminoPagoAsync(idTerminoPago, token);

                    TempData["MensajeExito"] =
                        "El término de pago se activó correctamente.";
                }
                else
                {
                    await _terminoPagoApiService
                        .DesactivarTerminoPagoAsync(idTerminoPago, token);

                    TempData["MensajeExito"] =
                        "El término de pago se desactivó correctamente.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] =
                    "El término de pago solicitado no existe.";
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["MensajeError"] =
                    "No fue posible cambiar el estado del término de pago debido a un conflicto.";
            }
            catch (InvalidOperationException)
            {
                TempData["MensajeError"] =
                    "No tiene permisos para realizar esta operación.";
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "Ocurrió un error al cambiar el estado del término de pago.";
            }

            return RedirectToPage(
                "/TerminoPago/Listado_TerminoPago");
        }
    }
}
