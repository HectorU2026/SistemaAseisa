using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion;

[Authorize(Roles = "Vendedor,Contador")]
public class DetalleModel(FacturaApiService servicio) : PageModel
{
    public FacturaDetalle? Factura { get; private set; }

    public string? MensajeExito { get; private set; }

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(int idFactura, CancellationToken ct)
    {
        MensajeExito = TempData["MensajeExito"] as string;
        var mensajeOperacion = TempData["MensajeError"] as string;

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
            Factura = await servicio.ObtenerFacturaAsync(token, idFactura, ct);
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
                ? "No se pudo consultar la factura."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar el detalle de la factura. Intente nuevamente.";
        }

        if (string.IsNullOrWhiteSpace(MensajeError))
            MensajeError = mensajeOperacion;

        return Page();
    }

    public Task<IActionResult> OnPostConfirmarAsync(
        int idFactura,
        string? versionFila,
        CancellationToken ct)
    {
        return EjecutarCambioEstadoAsync(
            idFactura,
            versionFila,
            servicio.ConfirmarFacturaAsync,
            ct);
    }

    public Task<IActionResult> OnPostRestablecerAsync(
        int idFactura,
        string? versionFila,
        CancellationToken ct)
    {
        // El API no impide restablecer una factura Emitida que ya tenga notas de crédito.
        return EjecutarCambioEstadoAsync(
            idFactura,
            versionFila,
            servicio.RestablecerFacturaAsync,
            ct);
    }

    private async Task<IActionResult> EjecutarCambioEstadoAsync(
        int idFactura,
        string? versionFila,
        Func<string, int, byte[], CancellationToken, Task<FacturaCambioEstadoRespuesta>> operacion,
        CancellationToken ct)
    {
        if (!User.IsInRole("Contador"))
            return Forbid();

        if (idFactura <= 0)
        {
            TempData["MensajeError"] = "La factura solicitada no es válida.";
            return RedirectToPage(new { idFactura });
        }

        if (!TryDecodificarVersion(versionFila, out var version))
        {
            TempData["MensajeError"] = "La versión de la factura es obligatoria.";
            return RedirectToPage(new { idFactura });
        }

        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        try
        {
            var resultado = await operacion(token, idFactura, version, ct);
            TempData["MensajeExito"] = string.IsNullOrWhiteSpace(resultado.Mensaje)
                ? "La operación se completó correctamente."
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
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["MensajeError"] = string.IsNullOrWhiteSpace(ex.Message)
                ? "La factura no existe."
                : ex.Message;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            TempData["MensajeError"] = string.IsNullOrWhiteSpace(ex.Message)
                ? "No se pudo completar la operación."
                : ex.Message;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["MensajeError"] = string.IsNullOrWhiteSpace(ex.Message)
                ? "La factura fue modificada por otro usuario."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            TempData["MensajeError"] =
                "No fue posible completar la operación. Intente nuevamente.";
        }

        return RedirectToPage(new { idFactura });
    }

    private static bool TryDecodificarVersion(string? versionFila, out byte[] version)
    {
        version = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(versionFila))
            return false;

        try
        {
            version = Convert.FromBase64String(versionFila);
        }
        catch (FormatException)
        {
            return false;
        }

        return version.Length == 8;
    }
}
