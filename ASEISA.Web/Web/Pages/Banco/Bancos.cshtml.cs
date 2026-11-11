using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Banco
{
    public class BancosModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        public List<CuentaBancariaResponse> Cuentas { get; set; } = new();
        public List<MovimientoBancarioItem> Movimientos { get; set; } = new();

        public BancosModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            var cliente = ObtenerClienteConToken();

            string endpointCuentas = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasBancarias");
            var respCuentas = await cliente.GetAsync(endpointCuentas);

            if (respCuentas.IsSuccessStatusCode)
            {
                var contenido = await respCuentas.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cuentas = JsonSerializer.Deserialize<List<CuentaBancariaResponse>>(contenido, opciones) ?? new();
            }

            string endpointMov = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerMovimientos");
            string urlMov = $"{endpointMov}?pagina=1&tamanoPagina=10";

            var respMov = await cliente.GetAsync(urlMov);
            if (respMov.IsSuccessStatusCode)
            {
                var contenidoMov = await respMov.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultado = JsonSerializer.Deserialize<MovimientoBancarioRespuesta>(contenidoMov, opciones);
                Movimientos = resultado?.Items?.ToList() ?? new();
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