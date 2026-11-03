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

        public async Task OnGetAsync()
        {
            var idEmpresaClaim = User.FindFirst("Empresa")?.Value;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCanceladasRechazadas");
            string url = $"{endpoint}?idEmpresa={idEmpresaClaim}";
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
