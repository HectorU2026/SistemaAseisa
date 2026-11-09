using Abstracciones.Interfaces.Reglas;
using Microsoft.AspNetCore.Mvc;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Ventas
{
    public class ventas_canceladasModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public List<CotizacionResponseDetallada> Cotizaciones { get; set; } = new();

        public ventas_canceladasModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync(string? busqueda, DateTime? fecha)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            if (string.IsNullOrEmpty(idEmpresa)) return;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCanceladasRechazadas");

            // Construimos la URL con los filtros
            string url = $"{endpoint}/{idEmpresa}";
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(busqueda))
                queryParams.Add($"busqueda={Uri.EscapeDataString(busqueda)}");

            if (fecha.HasValue)
                queryParams.Add($"fecha={fecha.Value:yyyy-MM-dd}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cotizaciones = JsonSerializer.Deserialize<List<CotizacionResponseDetallada>>(contenido, opciones) ?? new();
            }
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
