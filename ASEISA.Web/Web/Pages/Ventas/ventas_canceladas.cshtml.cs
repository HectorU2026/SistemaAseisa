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
        public List<CotizacionResponse> Cotizaciones { get; set; } = new();

        public ventas_canceladasModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCanceladasRechazadas");
            var cliente = new HttpClient();
            var respuesta = await cliente.GetAsync(endpoint);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cotizaciones = JsonSerializer.Deserialize<List<CotizacionResponse>>(contenido, opciones) ?? new();
            }
        }
    }
}
