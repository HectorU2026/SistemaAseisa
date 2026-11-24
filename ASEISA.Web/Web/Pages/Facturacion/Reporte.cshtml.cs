using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Facturacion;

[Authorize(Roles = "Contador")]
public class ReporteModel(FacturaApiService servicio) : PageModel
{
    private const int MaximoPeriodo = 20;
    private const int MaximoTipoFactura = 50;
    private static readonly string[] OrdenesPermitidas = ["periodo", "tipo_factura"];

    [BindProperty(SupportsGet = true)]
    public string? Periodo { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TipoFactura { get; set; }

    [BindProperty(SupportsGet = true)]
    public string OrdenarPor { get; set; } = "periodo";

    public IReadOnlyList<ReporteFacturacionGrupo> Grupos { get; private set; } =
        Array.Empty<ReporteFacturacionGrupo>();

    public string? Mensaje { get; private set; }

    public string? MensajeError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        Periodo = string.IsNullOrWhiteSpace(Periodo) ? null : Periodo.Trim();
        TipoFactura = string.IsNullOrWhiteSpace(TipoFactura) ? null : TipoFactura.Trim();
        OrdenarPor = string.IsNullOrWhiteSpace(OrdenarPor) ? "periodo" : OrdenarPor.Trim();

        if (Periodo is { Length: > MaximoPeriodo }
            || (Periodo != null && !EsPeriodoValido(Periodo)))
        {
            MensajeError = "El período debe tener el formato YYYY-MM.";
            return Page();
        }

        if (TipoFactura is { Length: > MaximoTipoFactura })
        {
            MensajeError = "El tipo de factura no puede superar 50 caracteres.";
            return Page();
        }

        if (!OrdenesPermitidas.Contains(OrdenarPor))
        {
            MensajeError = "El ordenamiento solo admite periodo o tipo_factura.";
            return Page();
        }

        try
        {
            var resultado = await servicio.ObtenerReporteFacturacionAsync(
                token,
                new ReporteFacturacionFiltro
                {
                    Periodo = Periodo,
                    TipoFactura = TipoFactura,
                    OrdenarPor = OrdenarPor
                },
                ct);

            Grupos = resultado.Grupos ?? new List<ReporteFacturacionGrupo>();
            if (Grupos.Count == 0)
            {
                Mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? "No hay registros disponibles"
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
                ? "No se pudo consultar el reporte de facturación."
                : ex.Message;
        }
        catch (Exception ex) when (FacturaApiService.EsErrorDeCarga(ex, ct))
        {
            MensajeError = "No fue posible cargar el reporte de facturación. Intente nuevamente.";
        }

        return Page();
    }

    private static bool EsPeriodoValido(string periodo) =>
        DateTime.TryParseExact(
            periodo,
            "yyyy-MM",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
}
