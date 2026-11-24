using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion;

[Authorize(Roles = "Contador")]
public class PagadasModel(FacturaApiService servicio) : PageModel
{
    private const int MaximoNombreCliente = 200;

    [BindProperty(SupportsGet = true)]
    public string? NombreCliente { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaPagoDesde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaPagoHasta { get; set; }

    public IReadOnlyList<FacturaPagadaListado> Facturas { get; private set; } =
        Array.Empty<FacturaPagadaListado>();

    public string? Mensaje { get; private set; }

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        if (TieneErrorDeBinding(nameof(FechaPagoDesde)) || TieneErrorDeBinding(nameof(FechaPagoHasta)))
        {
            MensajeError = "La fecha de pago no es válida.";
            return Page();
        }

        NombreCliente = string.IsNullOrWhiteSpace(NombreCliente) ? null : NombreCliente.Trim();

        if (NombreCliente is { Length: > MaximoNombreCliente })
        {
            MensajeError = "El nombre del cliente no puede superar 200 caracteres.";
            return Page();
        }

        if (FechaPagoDesde.HasValue
            && FechaPagoHasta.HasValue
            && FechaPagoDesde.Value.Date > FechaPagoHasta.Value.Date)
        {
            MensajeError = "El rango de fechas no es válido.";
            return Page();
        }

        try
        {
            var resultado = await servicio.ObtenerFacturasPagadasAsync(
                token,
                new FacturaPagadaFiltro
                {
                    NombreCliente = NombreCliente,
                    FechaPagoDesde = FechaPagoDesde,
                    FechaPagoHasta = FechaPagoHasta
                },
                ct);

            Facturas = resultado.Facturas ?? new List<FacturaPagadaListado>();
            if (Facturas.Count == 0)
            {
                Mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? "No hay facturas pagadas registradas"
                    : resultado.Mensaje;
            }
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
                ? "No se pudo consultar las facturas pagadas."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar las facturas pagadas. Intente nuevamente.";
        }

        return Page();
    }

    private bool TieneErrorDeBinding(string campo) =>
        ModelState.TryGetValue(campo, out var estado) && estado.Errors.Count > 0;
}
