using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Contabilidad;

[Authorize(Roles = "Administrador,Contador,Auxiliar")]
public class Index_ContabilidadModel(CuentaContableApiService servicio) : PageModel
{
    public bool ResumenDisponible { get; private set; }
    public int Activas { get; private set; }
    public int Inactivas { get; private set; }
    public int Raices { get; private set; }
    public List<TipoResumen> PorTipo { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) return RedirectToPage("/Seguridad/Login");
        try
        {
            var cuentas = await servicio.ObtenerAsync(token, ct);
            var activas = cuentas.Where(c => c.IdEstado == 1).ToList();
            Activas = activas.Count;
            Inactivas = cuentas.Count(c => c.IdEstado == 2);
            Raices = activas.Count(c => c.IdCuentaPrincipal is null);
            PorTipo = activas.GroupBy(c => string.IsNullOrWhiteSpace(c.TipoCuenta) ? "Sin tipo" : c.TipoCuenta)
                .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                .Select(g => new TipoResumen(g.Key, g.Count())).ToList();
            ResumenDisponible = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        { return RedirectToPage("/Seguridad/Login"); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        { return Forbid(); }
        catch (Exception ex) when (CuentaContableApiService.EsErrorDeCarga(ex, ct))
        { ResumenDisponible = false; }

        return Page();
    }

    public record TipoResumen(string Nombre, int Cantidad);
}
