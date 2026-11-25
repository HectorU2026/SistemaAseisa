using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.CuentasPorCobrar
{
    public class ListadoModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EstadoMorosidad { get; set; }

        public List<CuentaPorCobrarResponse> TodasLasCuentas { get; set; } = new();
        public List<CuentaPorCobrarResponse> CuentasFiltradas { get; set; } = new();

        public decimal SaldoTotalPendiente { get; set; }
        public int TotalFacturasAlDia { get; set; }
        public int TotalFacturasAtrasadas { get; set; }
        public string? MensajeError { get; set; }

        public ListadoModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            var contexto = ObtenerContextoSesion();
            if (contexto == null)
            {
                MensajeError = "No se pudo identificar la empresa o el usuario de la sesión.";
                return Page();
            }

            var (idEmpresa, idUsuario) = contexto.Value;

            var cliente = ObtenerClienteConToken();
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasPorCobrar");
            string url = $"{endpointBase}/{idEmpresa}/usuario/{idUsuario}";

            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                TodasLasCuentas = JsonSerializer.Deserialize<List<CuentaPorCobrarResponse>>(contenido, opciones) ?? new();

                // Calcular indicadores generales
                SaldoTotalPendiente = TodasLasCuentas.Sum(c => c.SaldoPendiente);
                TotalFacturasAlDia = TodasLasCuentas.Count(c => string.Equals(c.EstadoMorosidad, "Al día", StringComparison.OrdinalIgnoreCase));
                TotalFacturasAtrasadas = TodasLasCuentas.Count(c => string.Equals(c.EstadoMorosidad, "Atrasado", StringComparison.OrdinalIgnoreCase));

                // Aplicar filtros de búsqueda y estado
                IEnumerable<CuentaPorCobrarResponse> consulta = TodasLasCuentas;

                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    string termino = Busqueda.Trim();
                    consulta = consulta.Where(c =>
                        c.NombreCliente.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                        c.NumeroFactura.Contains(termino, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(EstadoMorosidad))
                {
                    consulta = consulta.Where(c =>
                        string.Equals(c.EstadoMorosidad, EstadoMorosidad, StringComparison.OrdinalIgnoreCase));
                }

                CuentasFiltradas = consulta.ToList();
            }
            else
            {
                MensajeError = "No se pudo cargar el listado de cuentas por cobrar.";
            }

            return Page();
        }

        private (int IdEmpresa, int IdUsuario)? ObtenerContextoSesion()
        {
            var usuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("IdUsuario")?.Value;

            var empresaStr = User.FindFirst("IdEmpresa")?.Value
                             ?? User.FindFirst("Empresa")?.Value;

            if (!int.TryParse(usuarioStr, out int idUsuario) || idUsuario <= 0)
                return null;

            // Si el claim "Empresa" contiene el ID numérico lo usamos; si contiene el nombre, usamos 1 por defecto
            if (!int.TryParse(empresaStr, out int idEmpresa) || idEmpresa <= 0)
            {
                idEmpresa = 1;
            }

            return (idEmpresa, idUsuario);
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