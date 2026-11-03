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

        public BancosModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            var idEmpresa = User.FindFirst("Empresa")?.Value;
            if (string.IsNullOrEmpty(idEmpresa))
                return;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasBancarias");
            string url = $"{endpoint}?idEmpresa={idEmpresa}";
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cuentas = JsonSerializer.Deserialize<List<CuentaBancariaResponse>>(contenido, opciones) ?? new();
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
