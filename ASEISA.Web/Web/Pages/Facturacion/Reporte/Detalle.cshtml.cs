using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion.Reporte;

[Authorize(Roles = "Contador")]
public class DetalleModel(FacturaApiService servicio) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Periodo { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TipoFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? OrdenarPor { get; set; }

    public FacturaDetalle? Factura { get; private set; }

    public IReadOnlyList<ReporteNotaCredito> NotasCredito { get; private set; } =
        Array.Empty<ReporteNotaCredito>();

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(int idFactura, CancellationToken ct)
    {
        Periodo = string.IsNullOrWhiteSpace(Periodo) ? null : Periodo.Trim();
        TipoFactura = string.IsNullOrWhiteSpace(TipoFactura) ? null : TipoFactura.Trim();
        OrdenarPor = string.IsNullOrWhiteSpace(OrdenarPor) ? null : OrdenarPor.Trim();

        if (idFactura <= 0)
        {
            MensajeError = "La factura solicitada no es válida.";
            return Page();
        }

        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        try
        {
            var detalle = await servicio.ObtenerDetalleReporteFacturacionAsync(token, idFactura, ct);
            Factura = detalle.Factura;
            NotasCredito = detalle.NotasCredito ?? new List<ReporteNotaCredito>();
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
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            MensajeError = string.IsNullOrWhiteSpace(ex.Message)
                ? "La factura no existe."
                : ex.Message;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            MensajeError = string.IsNullOrWhiteSpace(ex.Message)
                ? "No se pudo consultar el detalle del reporte."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar el detalle del reporte. Intente nuevamente.";
        }

        return Page();
    }
}
