using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;
using static Web.Servicios.CuentaContableApiService;

namespace Web.Pages.Contabilidad;

[Authorize(Roles = "Administrador,Contador,Auxiliar")]
public class Listado_CuentaContableModel(CuentaContableApiService servicio) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Busqueda { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Estado { get; set; }

    public List<CuentaFila> Cuentas { get; private set; } = new();
    public string? MensajeError { get; private set; }
    public bool PuedeCambiarEstado => User.IsInRole("Administrador") || User.IsInRole("Contador");

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) return RedirectToPage("/Seguridad/Login");
        try
        {
            IEnumerable<CuentaFila> cuentas = await servicio.ObtenerAsync(token, ct);
            if (Estado is 1 or 2) cuentas = cuentas.Where(c => c.IdEstado == Estado);
            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                var texto = Busqueda.Trim();
                cuentas = cuentas.Where(c => c.Codigo.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
                    || c.Nombre.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
                    || c.TipoCuenta.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
                    || (c.CuentaPrincipal?.Contains(texto, StringComparison.CurrentCultureIgnoreCase) ?? false));
            }
            Cuentas = cuentas.ToList();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        { return RedirectToPage("/Seguridad/Login"); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        { return Forbid(); }
        catch (Exception ex) when (EsErrorDeCarga(ex, ct))
        { MensajeError = "No fue posible cargar las cuentas contables. Intente nuevamente."; }
        return Page();
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync(int id, bool habilitar, CancellationToken ct)
    {
        if (!PuedeCambiarEstado) return Forbid();
        if (!ModelState.IsValid || id <= 0) return BadRequest();
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) return RedirectToPage("/Seguridad/Login");
        try
        {
            var error = await servicio.CambiarEstadoAsync(id, habilitar, token, ct);
            if (error is not null)
            {
                TempData["ErrorCuenta"] = error;
                return RedirectToPage();
            }

            TempData["MensajeCuenta"] = habilitar ? "Cuenta contable habilitada con éxito."
                : "Cuenta contable inhabilitada con éxito.";
            return RedirectToPage("./Listado_CuentaContable",
                new { Busqueda, Estado });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        { return RedirectToPage("/Seguridad/Login"); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        { return Forbid(); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        { TempData["ErrorCuenta"] = "No se encontró la cuenta contable."; }
        catch (Exception ex) when (EsErrorDeCarga(ex, ct))
        { TempData["ErrorCuenta"] = "No fue posible cambiar el estado, debido a que la cuenta raíz se encuentra inhabilitada."; }
        return RedirectToPage();
    }
}