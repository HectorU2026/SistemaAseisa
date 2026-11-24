using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Banco
{
    public class RegistrarPagoModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public PagoClienteRequest Pago { get; set; } = new();

        public List<CuentaBancariaResponse> CuentasBancarias { get; set; } = new();
        public List<CuentaPorCobrarResponse> CuentasPorCobrar { get; set; } = new();

        public string? MensajeError { get; set; }

        public RegistrarPagoModel(IConfiguracion configuracion)
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

            if (!Pago.IdCuentaCobrar.HasValue || Pago.IdCuentaCobrar <= 0
                || !Pago.IdCuentaBancaria.HasValue || Pago.IdCuentaBancaria <= 0
                || !Pago.FechaPago.HasValue
                || !Pago.Monto.HasValue || Pago.Monto <= 0
                || string.IsNullOrWhiteSpace(Pago.MetodoPago))
            {
                MensajeError = "Por favor, complete todos los campos obligatorios correctamente.";
                await CargarCatalogos();
                return Page();
            }

            // Requerido por BancoFlujo.RegistrarPagoCliente
            Pago.IdEstado = 1;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarPagoCliente");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(endpoint, Pago);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "Pago de factura registrado con éxito.";
                return RedirectToPage("/Banco/Bancos");
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "Ocurrió un error al registrar el pago."
                    : error.Trim('"');
                await CargarCatalogos();
                return Page();
            }
        }

        private async Task CargarCatalogos()
        {
            var cliente = ObtenerClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Cargar cuentas bancarias activas de la empresa
            string endpointCuentas = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasBancarias");
            var respCuentas = await cliente.GetAsync(endpointCuentas);
            if (respCuentas.IsSuccessStatusCode)
            {
                var contenido = await respCuentas.Content.ReadAsStringAsync();
                var todas = JsonSerializer.Deserialize<List<CuentaBancariaResponse>>(contenido, opciones) ?? new();
                CuentasBancarias = todas.Where(c => c.IdEstado == 1).ToList();
            }

            // 2. Cargar facturas pendientes (Cuentas por Cobrar)
            string endpointCxc = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasPorCobrar");
            if (!string.IsNullOrWhiteSpace(endpointCxc))
            {
                var respCxc = await cliente.GetAsync(endpointCxc);
                if (respCxc.IsSuccessStatusCode)
                {
                    var contenidoCxc = await respCxc.Content.ReadAsStringAsync();
                    CuentasPorCobrar = JsonSerializer.Deserialize<List<CuentaPorCobrarResponse>>(contenidoCxc, opciones) ?? new();
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