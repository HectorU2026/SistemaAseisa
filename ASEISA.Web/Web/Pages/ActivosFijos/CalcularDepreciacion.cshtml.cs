using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Web.Pages.ActivosFijos
{
    public class CalcularDepreciacionModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty(SupportsGet = true)]
        public int? IdActivoFijo { get; set; }

        [BindProperty]
        public CalcularDepreciacionRequest Calculo { get; set; } = new();

        public string? MensajeError { get; set; }

        public CalcularDepreciacionModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public IActionResult OnGet()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            if (IdActivoFijo.HasValue && IdActivoFijo > 0)
            {
                Calculo.IdActivoFijo = IdActivoFijo.Value;
            }

            if (string.IsNullOrWhiteSpace(Calculo.Periodo))
            {
                Calculo.Periodo = DateTime.Now.ToString("yyyy-MM");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            var contexto = ObtenerContextoSesion();
            if (contexto == null)
            {
                MensajeError = "No se pudo identificar la sesión del usuario.";
                return Page();
            }

            if (!Calculo.IdActivoFijo.HasValue || Calculo.IdActivoFijo <= 0
                || string.IsNullOrWhiteSpace(Calculo.Periodo))
            {
                MensajeError = "Complete todos los campos obligatorios.";
                return Page();
            }

            var (idEmpresa, idUsuario) = contexto.Value;
            Calculo.IdEmpresa = idEmpresa;
            Calculo.IdUsuario = idUsuario;
            Calculo.IdEstado = 1;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "CalcularDepreciacion");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(endpoint, Calculo);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Depreciación del periodo {Calculo.Periodo} calculada con éxito.";
                return RedirectToPage("/ActivosFijos/Listado", new { IdActivoFijo = Calculo.IdActivoFijo });
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "Ocurrió un error al calcular la depreciación."
                    : error.Trim('"');
                return Page();
            }
        }

        private (int IdEmpresa, int IdUsuario)? ObtenerContextoSesion()
        {
            var usuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("IdUsuario")?.Value;
            var empresaStr = User.FindFirst("IdEmpresa")?.Value
                             ?? User.FindFirst("Empresa")?.Value;

            if (!int.TryParse(usuarioStr, out int idUsuario) || idUsuario <= 0)
                return null;

            if (!int.TryParse(empresaStr, out int idEmpresa) || idEmpresa <= 0)
                idEmpresa = 1;

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
