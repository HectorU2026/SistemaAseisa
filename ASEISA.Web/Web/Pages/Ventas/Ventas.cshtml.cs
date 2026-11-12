using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using static Abstracciones.Modelos.Facturacion;

namespace Web.Pages.Ventas
{
    public class RespuestaPaginadaCotizacion
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int Total { get; set; }
        public List<CotizacionResponse> items { get; set; } = new();
    }

    public class VentasModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public List<CotizacionResponse> Cotizaciones { get; set; } = new();
        public List<TerminoPagoResponse> TerminosPago { get; set; } = new();

        public VentasModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync(string? busqueda, int? idEstado)
        {
            var idEmpresaClaim = User.FindFirst("Empresa")?.Value;

            if (string.IsNullOrEmpty(idEmpresaClaim))
            {
                RedirectToPage("/Seguridad/Login");
                return;
            }

            var cliente = ObtenerClienteConToken();

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCotizacionesFlujo");

            string url = $"{endpointBase}?idEmpresa={idEmpresaClaim}";

            if (!string.IsNullOrWhiteSpace(busqueda))
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

            if (idEstado.HasValue)
                url += $"&idEstado={idEstado}";

            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var resultado = JsonSerializer.Deserialize<RespuestaPaginadaCotizacion>(contenido, opciones);
                Cotizaciones = resultado?.items ?? new();
            }

            string endpointTerminos = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerTerminosPago");
            var respTerminos = await cliente.GetAsync(endpointTerminos);

            if (respTerminos.IsSuccessStatusCode)
            {
                var contenidoTerminos = await respTerminos.Content.ReadAsStringAsync();
                TerminosPago = JsonSerializer.Deserialize<List<TerminoPagoResponse>>(contenidoTerminos, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostCancelarAsync(int idCotizacion)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "CancelarCotizacionFlujo");
            string url = $"{endpointBase}?idEmpresa={idEmpresa}&idUsuario={idUsuario}&idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PutAsync(url, null);

            if (respuesta.IsSuccessStatusCode)
                TempData["Exito"] = "Cotización cancelada correctamente.";
            else
                TempData["Error"] = "Error al cancelar la cotización.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAceptarAsync(int idCotizacion)
        {
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "AvanzarCotizacionAOrdenDeVenta");
            string url = $"{endpointBase}?idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PutAsync(url, null);

            if (respuesta.IsSuccessStatusCode)
                TempData["Exito"] = "Cotización convertida a Orden de Venta.";
            else
                TempData["Error"] = "Error al aceptar la cotización.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRechazarAsync(int idCotizacion)
        {
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "RetrocederCotizacionEnviada");
            string url = $"{endpointBase}?idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PutAsync(url, null);

            if (respuesta.IsSuccessStatusCode)
                TempData["Exito"] = "Cotización devuelta a borrador.";
            else
                TempData["Error"] = "Error al rechazar la cotización.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEnviarAsync(int idCotizacion, string correoDestino)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cliente = ObtenerClienteConToken();

            string endpointEnviar = _configuracion.ObtenerMetodo("ApiEndPoints", "EnviarCotizacionPorCorreo");
            string urlEnviar = $"{endpointEnviar}?idCotizacion={idCotizacion}&correo={correoDestino}";
            var respEnviar = await cliente.PutAsync(urlEnviar, null);

            if (respEnviar.IsSuccessStatusCode)
            {
                string endpointAvanzar = _configuracion.ObtenerMetodo("ApiEndPoints", "AvanzarCotizacionEnviadaFlujo");
                string urlAvanzar = $"{endpointAvanzar}?idEmpresa={idEmpresa}&idUsuario={idUsuario}&idCotizacion={idCotizacion}";
                await cliente.PutAsync(urlAvanzar, null);

                TempData["Exito"] = "Cotización enviada y estado actualizado.";
            }
            else
            {
                TempData["Error"] = "Error al enviar la cotización.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFacturarAsync(int idCotizacion, FacturarRequest request)
        {
            if (request.FechaFactura == null)
            {
                request.FechaFactura = DateTime.Now;
            }

            if (request.CondicionVenta == "01")
            {
                request.IdTerminoPago = 0;
            }

            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "FacturarOrdenDeVenta");
            string url = $"{endpointBase}/{idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(url, request);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "Comprobante generado correctamente.";
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                TempData["Error"] = "Error al facturar: " + error;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRetrocederAsync(int idCotizacion)
        {
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "RetrocederAEnviada");
            string url = $"{endpointBase}?idCotizacion={idCotizacion}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PutAsync(url, null);

            if (respuesta.IsSuccessStatusCode)
                TempData["Exito"] = "Cotización retrocedida a 'Cotización Enviada' correctamente.";
            else
                TempData["Error"] = "Error al retroceder la cotización.";

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
