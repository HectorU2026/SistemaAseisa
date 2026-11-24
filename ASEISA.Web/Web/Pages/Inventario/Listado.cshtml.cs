using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Inventario;

[Authorize(Roles = "Administrador,Contador,Auxiliar,Comprador,Vendedor")]
public class ListadoModel(ProductoApiService servicio) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Nombre { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int TamanoPagina { get; set; } = 10;

    public IReadOnlyList<ProductoCatalogoItem> Items { get; private set; } =
        Array.Empty<ProductoCatalogoItem>();

    public int Total { get; private set; }

    public string? MensajeError { get; private set; }

    public bool PuedeVerCostos =>
        User.IsInRole("Administrador")
        || User.IsInRole("Contador")
        || User.IsInRole("Comprador");

    public int TotalPaginas =>
        TamanoPagina <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(Total / (double)TamanoPagina));

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        var empresa = User.FindFirst("Empresa")?.Value;
        if (!int.TryParse(empresa, out var idEmpresa) || idEmpresa <= 0)
            return RedirectToPage("/Seguridad/Login");

        if (Pagina < 1)
            Pagina = 1;

        if (TamanoPagina < 1)
            TamanoPagina = 10;

        if (TamanoPagina > 100)
            TamanoPagina = 100;

        Nombre = string.IsNullOrWhiteSpace(Nombre) ? null : Nombre.Trim();

        try
        {
            var respuesta = await servicio.ObtenerProductosAsync(
                token,
                idEmpresa,
                Nombre,
                Pagina,
                TamanoPagina,
                ct);

            Items = respuesta.Items ?? new List<ProductoCatalogoItem>();
            Total = respuesta.Total;

            if (respuesta.Pagina > 0)
                Pagina = respuesta.Pagina;

            if (respuesta.TamanoPagina > 0)
                TamanoPagina = respuesta.TamanoPagina;
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Seguridad/Login");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Seguridad/Login");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            MensajeError = string.IsNullOrWhiteSpace(ex.Message)
                ? "No se pudo consultar los productos."
                : ex.Message;
        }
        catch (Exception ex) when (ProductoApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar los productos. Intente nuevamente.";
        }

        return Page();
    }
}
