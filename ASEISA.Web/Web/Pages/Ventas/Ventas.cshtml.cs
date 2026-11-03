using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using static Abstracciones.Modelos.Facturacion;

namespace Web.Pages.Ventas
{
    public class VentasModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public List<CotizacionResponse> Cotizaciones { get; set; } = new();

        public VentasModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            var idEmpresaClaim = User.FindFirst("Empresa")?.Value;

            if (string.IsNullOrEmpty(idEmpresaClaim))
            {
                RedirectToPage("/Seguridad/Login");
                return;
            }

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCotizacionesFlujo");
            string url = $"{endpointBase}?idEmpresa={idEmpresaClaim}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cotizaciones = JsonSerializer.Deserialize<List<CotizacionResponse>>(contenido, opciones) ?? new();
            }
        }

        public async Task<IActionResult> OnPostCancelarAsync(int idCotizacion)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "CancelarCotizacionFlujo");
            string url = $"{endpointBase}?idEmpresa={idEmpresa}&idUsuario={idUsuario}&idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            await cliente.PutAsync(url, null);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAceptarAsync(int idCotizacion)
        {
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "AvanzarCotizacionAOrdenDeVenta");
            string url = $"{endpointBase}?idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            await cliente.PutAsync(url, null);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRechazarAsync(int idCotizacion)
        {
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "RetrocederCotizacionEnviada");
            string url = $"{endpointBase}?idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            await cliente.PutAsync(url, null);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEnviarAsync(int idCotizacion, string correoDestino)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cliente = ObtenerClienteConToken();

            string endpointEnviar = _configuracion.ObtenerMetodo("ApiEndPoints", "EnviarCotizacionPorCorreo");
            string urlEnviar = $"{endpointEnviar}?idCotizacion={idCotizacion}&correo={correoDestino}";

            var respuestaEnviar = await cliente.PutAsync(urlEnviar, null);

            if (respuestaEnviar.IsSuccessStatusCode)
            {
                string endpointAvanzar = _configuracion.ObtenerMetodo("ApiEndPoints", "AvanzarCotizacionEnviadaFlujo");
                string urlAvanzar = $"{endpointAvanzar}?idEmpresa={idEmpresa}&idUsuario={idUsuario}&idCotizacion={idCotizacion}";

                await cliente.PutAsync(urlAvanzar, null);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFacturarAsync(int idCotizacion, FacturarRequest request)
        {
            if (request.FechaFactura == null)
            {
                request.FechaFactura = DateTime.Now;
            }

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "FacturarOrdenDeVenta");
            string url = $"{endpointBase}/{idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(url, request);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "Factura generada correctamente.";
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                TempData["Error"] = "Error al facturar: " + error;
            }

            return RedirectToPage();
        }

        private HttpClient ObtenerClienteConToken()
        {
            var tokenClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Token");
            var cliente = new HttpClient();

            if (tokenClaim != null)
            {
                cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClaim.Value);
            }

            return cliente;
        }
    }
}
