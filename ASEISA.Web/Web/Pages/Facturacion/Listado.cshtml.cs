using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion;

[Authorize(Roles = "Vendedor,Contador")]
public class ListadoModel(FacturaApiService servicio) : PageModel
{
    private const int MaximoNumeroFactura = 50;
    private const int MaximoNombreCliente = 200;

    [BindProperty(SupportsGet = true)]
    public string? NumeroFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? NombreCliente { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? IdEstadoFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaEmisionDesde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaEmisionHasta { get; set; }

    public IReadOnlyList<FacturaListado> Facturas { get; private set; } =
        Array.Empty<FacturaListado>();

    public IReadOnlyList<EstadoFactura> Estados { get; private set; } =
        Array.Empty<EstadoFactura>();

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        NumeroFactura = string.IsNullOrWhiteSpace(NumeroFactura) ? null : NumeroFactura.Trim();
        NombreCliente = string.IsNullOrWhiteSpace(NombreCliente) ? null : NombreCliente.Trim();

        if (IdEstadoFactura is <= 0)
            IdEstadoFactura = null;

        try
        {
            Estados = await servicio.ObtenerEstadosFacturaAsync(token, ct);
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
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar los estados de factura. Intente nuevamente.";
            return Page();
        }

        if (NumeroFactura is { Length: > MaximoNumeroFactura })
        {
            MensajeError = "El número de factura no puede superar 50 caracteres.";
            return Page();
        }

        if (NombreCliente is { Length: > MaximoNombreCliente })
        {
            MensajeError = "El nombre del cliente no puede superar 200 caracteres.";
            return Page();
        }

        if (FechaEmisionDesde.HasValue
            && FechaEmisionHasta.HasValue
            && FechaEmisionDesde.Value.Date > FechaEmisionHasta.Value.Date)
        {
            MensajeError = "La fecha de emisión desde no puede ser posterior a la fecha de emisión hasta.";
            return Page();
        }

        if (IdEstadoFactura.HasValue
            && Estados.All(estado => estado.IdEstadoFactura != IdEstadoFactura.Value))
        {
            MensajeError = "El estado seleccionado no es válido.";
            return Page();
        }

        try
        {
            Facturas = await servicio.ObtenerFacturasAsync(
                token,
                new FacturaFiltro
                {
                    NumeroFactura = NumeroFactura,
                    NombreCliente = NombreCliente,
                    IdEstadoFactura = IdEstadoFactura,
                    FechaEmisionDesde = FechaEmisionDesde,
                    FechaEmisionHasta = FechaEmisionHasta
                },
                ct);
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
                ? "No se pudo consultar las facturas."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar las facturas. Intente nuevamente.";
        }

        return Page();
    }
}
