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

        public async Task OnGetAsync()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasPorPagar");
            var cliente = new HttpClient();
            var respuesta = await cliente.GetAsync(endpoint);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cuentas = JsonSerializer.Deserialize<List<CuentaPorPagar>>(contenido, opciones) ?? new();
            }
        }
    }
}
