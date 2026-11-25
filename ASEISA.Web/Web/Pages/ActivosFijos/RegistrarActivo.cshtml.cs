using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.ActivosFijos
{
    public class RegistrarActivoModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public ActivoFijoRequest Activo { get; set; } = new();

        public List<CuentaContableResponse> CuentasContables { get; set; } = new();
        public string? MensajeError { get; set; }

        public RegistrarActivoModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            await CargarCuentasContables();
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
                await CargarCuentasContables();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Activo.Codigo)
                || string.IsNullOrWhiteSpace(Activo.Nombre)
                || !Activo.FechaAdquisicion.HasValue
                || !Activo.CostoAdquisicion.HasValue || Activo.CostoAdquisicion <= 0
                || !Activo.ValorResidual.HasValue || Activo.ValorResidual < 0
                || !Activo.VidaUtilMeses.HasValue || Activo.VidaUtilMeses <= 0
                || string.IsNullOrWhiteSpace(Activo.MetodoDepreciacion)
                || !Activo.IdCuentaActivo.HasValue || Activo.IdCuentaActivo <= 0
                || !Activo.IdCuentaDepreciacion.HasValue || Activo.IdCuentaDepreciacion <= 0)
            {
                MensajeError = "Por favor, complete todos los campos obligatorios correctamente.";
                await CargarCuentasContables();
                return Page();
            }

            var (idEmpresa, idUsuario) = contexto.Value;
            Activo.IdEmpresa = idEmpresa;
            Activo.IdUsuario = idUsuario;
            Activo.IdEstado = 1;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarActivoFijo");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(endpoint, Activo);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "Activo registrado con éxito.";
                return RedirectToPage("/ActivosFijos/Listado");
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "Ocurrió un error al registrar el activo."
                    : error.Trim('"');
                await CargarCuentasContables();
                return Page();
            }
        }

        private async Task CargarCuentasContables()
        {
            var cliente = ObtenerClienteConToken();
            string endpointContables = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCuentasContables");
            var respContables = await cliente.GetAsync(endpointContables);
            if (respContables.IsSuccessStatusCode)
            {
                var contenido = await respContables.Content.ReadAsStringAsync();
                CuentasContables = JsonSerializer.Deserialize<List<CuentaContableResponse>>(
                    contenido, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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