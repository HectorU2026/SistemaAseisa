using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.MetodoPago
{
    public class Listado_MetodoPagoModel : PageModel
    {
        private readonly MetodoPagoApiService _metodoPagoApiService;

        public Listado_MetodoPagoModel(
            MetodoPagoApiService metodoPagoApiService)
        {
            _metodoPagoApiService = metodoPagoApiService;
        }

        public List<MetodoPagoResponse> MetodosPago { get; set; } = new();

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
                var metodos = await _metodoPagoApiService
                    .ObtenerMetodosPagoAsync(token);

                // Filtrar por nombre
                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    string busqueda = Busqueda.Trim();

                    metodos = metodos.Where(m =>
                        m.Nombre.Contains(
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
                        metodos = metodos
                            .Where(m => m.Activo)
                            .ToList();
                    }
                    else if (Estado.Equals(
                        "Inactivo",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        metodos = metodos
                            .Where(m => !m.Activo)
                            .ToList();
                    }
                }

                MetodosPago = metodos
                    .OrderBy(m => m.Nombre)
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (InvalidOperationException)
            {
                MensajeError =
                    "No tiene permisos para consultar los métodos de pago.";
            }
            catch (Exception)
            {
                MensajeError =
                    "No fue posible cargar los métodos de pago. Intente nuevamente.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostActivarAsync(
            int idMetodoPago)
        {
            return await CambiarEstadoAsync(
                idMetodoPago,
                activar: true);
        }

        public async Task<IActionResult> OnPostDesactivarAsync(
            int idMetodoPago)
        {
            return await CambiarEstadoAsync(
                idMetodoPago,
                activar: false);
        }

        private async Task<IActionResult> CambiarEstadoAsync(
            int idMetodoPago,
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

            if (idMetodoPago <= 0)
            {
                TempData["MensajeError"] =
                    "El identificador del método de pago no es válido.";

                return RedirectToPage(
                    "/MetodoPago/Listado_MetodoPago");
            }

            try
            {
                if (activar)
                {
                    await _metodoPagoApiService
                        .ActivarMetodoPagoAsync(idMetodoPago, token);

                    TempData["MensajeExito"] =
                        "El método de pago se activó correctamente.";
                }
                else
                {
                    await _metodoPagoApiService
                        .DesactivarMetodoPagoAsync(idMetodoPago, token);

                    TempData["MensajeExito"] =
                        "El método de pago se desactivó correctamente.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] =
                    "El método de pago solicitado no existe.";
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["MensajeError"] =
                    "No fue posible cambiar el estado del método de pago debido a un conflicto.";
            }
            catch (InvalidOperationException)
            {
                TempData["MensajeError"] =
                    "No tiene permisos para realizar esta operación.";
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "Ocurrió un error al cambiar el estado del método de pago.";
            }

            return RedirectToPage(
                "/MetodoPago/Listado_MetodoPago");
        }
    }
}
