using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.CuentasPorCobrar
{
    public class CuentasPorPagarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public List<CuentaPorPagar> Cuentas { get; set; } = new();

        public CuentasPorPagarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync(string? busqueda, int? idEstado, DateTime? fecha)
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            if (string.IsNullOrEmpty(idEmpresa)) return;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasPorPagar");

            string url = $"{endpoint}?idEmpresa={idEmpresa}";

            if (!string.IsNullOrWhiteSpace(busqueda))
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

            if (idEstado.HasValue)
                url += $"&idEstado={idEstado}";

            if (fecha.HasValue)
                url += $"&fechaVencimiento={fecha.Value:yyyy-MM-dd}";

            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cuentas = JsonSerializer.Deserialize<List<CuentaPorPagar>>(contenido, opciones) ?? new();
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
