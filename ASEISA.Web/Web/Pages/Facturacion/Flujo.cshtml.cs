using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion;

[Authorize(Roles = "Vendedor,Contador")]
public class FlujoModel(FacturaApiService servicio) : PageModel
{
    private const int MaximoNumeroFactura = 50;
    private static readonly string[] OrdenesPermitidas = ["id_cotizacion", "numeroFactura"];

    [BindProperty(SupportsGet = true)]
    public int? IdCotizacion { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? NumeroFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public string OrdenarPor { get; set; } = "id_cotizacion";

    public IReadOnlyList<FlujoFacturaListado> Items { get; private set; } =
        Array.Empty<FlujoFacturaListado>();

    public string? Mensaje { get; private set; }

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        if (ModelState.TryGetValue(nameof(IdCotizacion), out var estadoCotizacion)
            && estadoCotizacion.Errors.Count > 0)
        {
            MensajeError = "El identificador de cotización no es válido.";
            return Page();
        }

        if (IdCotizacion is <= 0)
        {
            MensajeError = "El identificador de cotización no es válido.";
            return Page();
        }

        NumeroFactura = string.IsNullOrWhiteSpace(NumeroFactura) ? null : NumeroFactura.Trim();
        OrdenarPor = string.IsNullOrWhiteSpace(OrdenarPor) ? "id_cotizacion" : OrdenarPor.Trim();

        if (NumeroFactura is { Length: > MaximoNumeroFactura })
        {
            MensajeError = "El número de factura no puede superar 50 caracteres.";
            return Page();
        }

        if (!OrdenesPermitidas.Contains(OrdenarPor))
        {
            MensajeError = "El ordenamiento solo admite id_cotizacion o numeroFactura.";
            return Page();
        }

        try
        {
            var resultado = await servicio.ObtenerFlujoFacturasAsync(
                token,
                new FlujoFacturaFiltro
                {
                    IdCotizacion = IdCotizacion,
                    NumeroFactura = NumeroFactura,
                    OrdenarPor = OrdenarPor
                },
                ct);

            Items = resultado.Items ?? new List<FlujoFacturaListado>();
            if (Items.Count == 0)
                Mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? "No hay registros disponibles"
                    : resultado.Mensaje;
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
                ? "No se pudo consultar el flujo de facturación."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar el flujo de facturación. Intente nuevamente.";
        }

        return Page();
    }
}
