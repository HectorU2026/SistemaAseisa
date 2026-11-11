using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Banco
{
    public class AgregarCuentaBancariaModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public CuentaBancariaRequest Cuenta { get; set; } = new();

        public List<TipoCuentaResponse> TiposCuenta { get; set; } = new();
        public List<CuentaContableResponse> CuentasContables { get; set; } = new();

        public string? MensajeError { get; set; }

        public AgregarCuentaBancariaModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            await CargarCatalogos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            if (string.IsNullOrEmpty(Cuenta.NumeroCuenta) || Cuenta.IdTipoCuenta == 0 || Cuenta.IdCuentaContable == 0)
            {
                MensajeError = "Por favor, complete los campos obligatorios.";
                await CargarCatalogos();
                return Page();
            }

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarCuentaBancaria");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(endpoint, Cuenta);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "Cuenta bancaria registrada correctamente.";
                return RedirectToPage("/Banco/Bancos");
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = "Error al registrar la cuenta: " + error;
                await CargarCatalogos();
                return Page();
            }
        }

        private async Task CargarCatalogos()
        {
            var cliente = ObtenerClienteConToken();

            string endpointTipos = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerTiposCuenta");
            var respTipos = await cliente.GetAsync(endpointTipos);
            if (respTipos.IsSuccessStatusCode)
            {
                var contenido = await respTipos.Content.ReadAsStringAsync();
                TiposCuenta = JsonSerializer.Deserialize<List<TipoCuentaResponse>>(contenido, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            string endpointContables = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasContables");
            var respContables = await cliente.GetAsync(endpointContables);
            if (respContables.IsSuccessStatusCode)
            {
                var contenido = await respContables.Content.ReadAsStringAsync();
                CuentasContables = JsonSerializer.Deserialize<List<CuentaContableResponse>>(contenido, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        private HttpClient ObtenerClienteConToken()
        {
            var tokenClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Token");
            var cliente = new HttpClient();
            if (tokenClaim != null)
            {
                cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClaim.Value);
            }
            return cliente;
        }
    }
}
