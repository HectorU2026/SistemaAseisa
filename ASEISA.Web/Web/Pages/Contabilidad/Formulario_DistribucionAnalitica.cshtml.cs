using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Web.Servicios;

namespace Web.Pages.Contabilidad;

[Authorize(Roles = "Administrador,Contador,Auxiliar")]
public class Formulario_DistribucionAnaliticaModel(DistribucionAnaliticaApiService servicio) : PageModel
{
    public int? Id { get; private set; }
    public bool PuedeGuardar { get; private set; }
    public List<SelectListItem> Proveedores { get; private set; } = new();
    public List<SelectListItem> Productos { get; private set; } = new();

    [BindProperty]
    public Campos Datos { get; set; } = new();

    public class Campos
    {
        [Required(ErrorMessage = "Seleccione un proveedor.")]
        [Range(1, int.MaxValue)]
        public int? IdProveedor { get; set; }
        [Required(ErrorMessage = "Seleccione un producto.")]
        [Range(1, int.MaxValue)]
        public int? IdProducto { get; set; }
        [Required(ErrorMessage = "Ingrese el importe.")]
        public string? Importe { get; set; }
        [StringLength(300)] public string? Descripcion { get; set; }
        [StringLength(100)] public string? Referencia { get; set; }
        [StringLength(50)] public string? Unidad { get; set; }
    }

    public Task<IActionResult> OnGetAsync(int? id, CancellationToken cancelacion) =>
        ProcesarAsync(id, false, cancelacion);

    public Task<IActionResult> OnPostAsync(int? id, CancellationToken cancelacion) =>
        ProcesarAsync(id, true, cancelacion);

    private async Task<IActionResult> ProcesarAsync(int? id, bool guardar, CancellationToken cancelacion)
    {
        Id = id;
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) return RedirectToPage("/Seguridad/Login");

        try
        {
            if (id.HasValue)
            {
                var actual = await servicio.ObtenerDetalleAsync(id.Value, token, cancelacion);
                if (actual is null) return NotFound();
                if (!actual.Activo)
                {
                    TempData["MensajeError"] = "No se puede editar una distribución inactiva.";
                    return RedirectToPage("./Listado_DistribucionAnalitica");
                }

                if (!guardar)
                    Datos = new Campos
                    {
                        IdProveedor = actual.IdProveedor,
                        IdProducto = actual.IdProducto,
                        Importe = actual.Importe.ToString("0.00", CultureInfo.InvariantCulture),
                        Descripcion = actual.Descripcion,
                        Referencia = actual.Referencia,
                        Unidad = actual.Unidad
                    };
            }

            var catalogos = await servicio.ObtenerCatalogosAsync(token, cancelacion);
            Proveedores = catalogos.Proveedores.Select(p => new SelectListItem(p.Nombre, p.IdProveedor.ToString())).ToList();
            Productos = catalogos.Productos.Select(p => new SelectListItem(p.Nombre, p.IdProducto.ToString())).ToList();
            PuedeGuardar = Proveedores.Count > 0 && Productos.Count > 0;
            if (!PuedeGuardar)
            {
                ModelState.AddModelError("", "Se requiere al menos un proveedor y un producto activos.");
                return Page();
            }
            if (!guardar) return Page();

            var texto = Datos.Importe?.Trim().Replace(',', '.');
            if (!decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var importe)
                || importe < -9999999999999999.99m || importe > 9999999999999999.99m
                || decimal.Round(importe, 2) != importe)
                ModelState.AddModelError("Datos.Importe", "Ingrese un número con hasta 16 enteros y 2 decimales, sin separadores de miles.");

            if (!ModelState.IsValid) return Page();
            var error = await servicio.GuardarAsync(id, new DistribucionAnaliticaRequest(
                Datos.IdProveedor!.Value, Datos.IdProducto!.Value, importe,
                Limpiar(Datos.Descripcion), Limpiar(Datos.Referencia), Limpiar(Datos.Unidad)),
                token, cancelacion);
            if (error is not null)
            {
                ModelState.AddModelError("", error);
                return Page();
            }
            TempData["MensajeExito"] = id.HasValue
                ? "Distribución analítica editada con éxito"
                : "Distribución analítica creada con éxito";
            return RedirectToPage("./Listado_DistribucionAnalitica");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Seguridad/Login");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound && id.HasValue)
        {
            return NotFound();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["MensajeError"] = "La distribución ya no está disponible para edición.";
            return RedirectToPage("./Listado_DistribucionAnalitica");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException
                                   || ex is OperationCanceledException && !cancelacion.IsCancellationRequested)
        {
            ModelState.AddModelError("", "No fue posible completar la operación. Intente nuevamente.");
            return Page();
        }
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}