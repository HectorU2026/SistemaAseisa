using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion.Flujo;

[Authorize(Roles = "Vendedor,Contador")]
public class DetalleModel(FacturaApiService servicio) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? IdCotizacion { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? NumeroFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? OrdenarPor { get; set; }

    public FacturaDetalle? Factura { get; private set; }

    public FlujoFacturaListado? Resumen { get; private set; }

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(int idFactura, CancellationToken ct)
    {
        if (IdCotizacion is <= 0)
            IdCotizacion = null;

        NumeroFactura = string.IsNullOrWhiteSpace(NumeroFactura) ? null : NumeroFactura.Trim();
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
            var detalle = await servicio.ObtenerFlujoFacturaAsync(token, idFactura, ct);
            Factura = detalle.Factura;
            Resumen = detalle.Resumen;
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
                ? "No se pudo consultar el flujo de facturación."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar el detalle del flujo. Intente nuevamente.";
        }

        return Page();
    }
}
