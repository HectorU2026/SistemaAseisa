using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Banco
{
    public class MovimientosModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty(SupportsGet = true)]
        public int? IdCuentaBancaria { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int TamanoPagina { get; set; } = 10;

        public List<MovimientoBancarioItem> Movimientos { get; set; } = new();
        public List<CuentaBancariaResponse> CuentasBancarias { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public string? MensajeError { get; set; }

        public MovimientosModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            if (Pagina < 1) Pagina = 1;
            if (TamanoPagina < 1) TamanoPagina = 10;
            if (TamanoPagina > 100) TamanoPagina = 100;

            var cliente = ObtenerClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Cargar cuentas bancarias para el filtro desplegable
            string endpointCuentas = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasBancarias");
            var respCuentas = await cliente.GetAsync(endpointCuentas);
            if (respCuentas.IsSuccessStatusCode)
            {
                var contenidoCuentas = await respCuentas.Content.ReadAsStringAsync();
                CuentasBancarias = JsonSerializer.Deserialize<List<CuentaBancariaResponse>>(contenidoCuentas, opciones) ?? new();
            }

            // 2. Consultar movimientos paginados
            string endpointMov = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerMovimientos");
            string urlMov = $"{endpointMov}?pagina={Pagina}&tamanoPagina={TamanoPagina}";

            if (IdCuentaBancaria.HasValue && IdCuentaBancaria.Value > 0)
            {
                urlMov += $"&idCuentaBancaria={IdCuentaBancaria.Value}";
            }

            var respMov = await cliente.GetAsync(urlMov);
            if (respMov.IsSuccessStatusCode)
            {
                var contenidoMov = await respMov.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<MovimientoBancarioRespuesta>(contenidoMov, opciones);

                if (resultado != null)
                {
                    Movimientos = resultado.Items?.ToList() ?? new();
                    TotalRegistros = resultado.Total;
                    TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
                }
            }
            else
            {
                MensajeError = "No se pudo cargar el historial de movimientos bancarios.";
            }

            return Page();
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