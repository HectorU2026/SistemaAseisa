using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text.Json;
using Web.Servicios;

namespace Web.Pages.Contabilidad;

[Authorize(Roles = "Administrador,Contador,Auxiliar")]
public class Listado_DistribucionAnaliticaModel(DistribucionAnaliticaApiService servicio) : PageModel
{
    public List<DistribucionAnaliticaResponse> Distribuciones { get; private set; } = new();
    public string? MensajeError { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Busqueda { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Estado { get; set; }

    public bool PuedeInhabilitar => User.IsInRole("Administrador") || User.IsInRole("Contador");

    public async Task<IActionResult> OnGetAsync(CancellationToken cancelacion)
    {
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        try
        {
            IEnumerable<DistribucionAnaliticaResponse> filas = await servicio.ObtenerAsync(token, cancelacion);
            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                var texto = Busqueda.Trim();
                filas = filas.Where(d => Contiene(d.Descripcion, texto) || Contiene(d.Referencia, texto)
                    || Contiene(d.Proveedor, texto) || Contiene(d.Producto, texto));
            }

            filas = Estado switch
            {
                "Activo" => filas.Where(d => d.Activo),
                "Inactivo" => filas.Where(d => !d.Activo),
                _ => filas
            };
            Distribuciones = filas.ToList();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Seguridad/Login");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException
                                   || ex is OperationCanceledException && !cancelacion.IsCancellationRequested)
        {
            MensajeError = "No fue posible cargar las distribuciones. Intente nuevamente.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostInhabilitarAsync(int idDistribucion, CancellationToken cancelacion)
    {
        if (!PuedeInhabilitar)
            return Forbid();

        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/Seguridad/Login");

        if (!ModelState.IsValid || idDistribucion <= 0)
            TempData["MensajeError"] = "El identificador de la distribución no es válido.";
        else
        {
            try
            {
                await servicio.InhabilitarAsync(idDistribucion, token, cancelacion);
                TempData["MensajeExito"] = "Distribución analítica inhabilitada con éxito";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }
            catch (HttpRequestException ex)
            {
                TempData["MensajeError"] = ex.StatusCode switch
                {
                    HttpStatusCode.NotFound => "La distribución solicitada no existe.",
                    HttpStatusCode.Conflict => "La distribución ya está inactiva.",
                    _ => "No fue posible inhabilitar la distribución."
                };
            }
            catch (Exception ex) when (ex is InvalidOperationException
                                       || ex is OperationCanceledException && !cancelacion.IsCancellationRequested)
            {
                TempData["MensajeError"] = "No fue posible inhabilitar la distribución.";
            }
        }

        return RedirectToPage("./Listado_DistribucionAnalitica",
            new { Busqueda, Estado });
    }

    private static bool Contiene(string? valor, string texto) =>
        valor?.Contains(texto, StringComparison.OrdinalIgnoreCase) == true;
}
