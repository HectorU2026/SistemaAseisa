using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.ActivosFijos
{
    public class ListadoModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty(SupportsGet = true)]
        public int? IdActivoFijo { get; set; }

        public List<DepreciacionHistorialResponse> Historial { get; set; } = new();
        public string? MensajeError { get; set; }

        public ListadoModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            if (!IdActivoFijo.HasValue || IdActivoFijo <= 0)
                return Page();

            int idEmpresa = ObtenerIdEmpresa();
            var cliente = ObtenerClienteConToken();
            string endpointBase = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerHistorialDepreciacion");
            string url = $"{endpointBase}/{idEmpresa}/activo/{IdActivoFijo.Value}";

            var respuesta = await cliente.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<DepreciacionHistorialResponse>>(contenido, opciones) ?? new();

                // Ordenar del periodo más reciente al más antiguo para mostrar el valor actual primero
                Historial = lista.OrderByDescending(h => h.Periodo)
                                 .ThenByDescending(h => h.IdDepreciacion)
                                 .ToList();
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "No se pudo consultar el historial de depreciación del activo."
                    : error.Trim('"');
            }

            return Page();
        }

        private int ObtenerIdEmpresa()
        {
            var empresaStr = User.FindFirst("IdEmpresa")?.Value
                             ?? User.FindFirst("Empresa")?.Value;

            return int.TryParse(empresaStr, out int idEmpresa) && idEmpresa > 0 ? idEmpresa : 1;
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