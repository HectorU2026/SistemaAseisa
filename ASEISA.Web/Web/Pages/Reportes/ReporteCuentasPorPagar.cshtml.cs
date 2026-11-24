using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Web.Helpers;

namespace Web.Pages.Reportes
{
    public class ReporteCuentasPorPagarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        public ReporteCuentaPorPagarResponse Reporte { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? OrdenarPor { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Ascendente { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public bool SoloVencidas { get; set; } = false;

        public ReporteCuentasPorPagarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            var cliente = ObtenerClienteConToken();
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ReporteCuentaPorPagar");

            string url = $"{endpoint}?soloVencidas={SoloVencidas}";
            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Reporte = JsonSerializer.Deserialize<ReporteCuentaPorPagarResponse>(contenido, opciones) ?? new();

                if (!string.IsNullOrEmpty(OrdenarPor))
                {
                    Reporte.Proveedores = OrdenarReportes.Ordenar(Reporte.Proveedores, OrdenarPor, Ascendente);
                }
                else
                {
                    // Por defecto, ordenamos alfabéticamente
                    Reporte.Proveedores = OrdenarReportes.Ordenar(Reporte.Proveedores, "NombreProveedor", true);
                }
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