using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Web.Servicios;
using static Web.Servicios.CuentaContableApiService;

namespace Web.Pages.Contabilidad;

[Authorize(Roles = "Administrador,Contador,Auxiliar")]
public class Formulario_CuentaContableModel(CuentaContableApiService servicio) : PageModel
{
    [BindProperty]
    public CuentaDatos Datos { get; set; } = new();
    public int? Id { get; private set; }
    public bool EsRaiz { get; private set; }
    public bool RequierePrincipal { get; private set; }
    public bool PuedeGuardar { get; private set; }
    public List<SelectListItem> Principales { get; private set; } = new();

    public Task<IActionResult> OnGetAsync(int? id, bool raiz, CancellationToken ct) => ProcesarAsync(id, raiz, false, ct);
    public Task<IActionResult> OnPostAsync(int? id, bool raiz, CancellationToken ct) => ProcesarAsync(id, raiz, true, ct);

    private async Task<IActionResult> ProcesarAsync(int? id, bool raiz, bool guardar, CancellationToken ct)
    {
        Id = id;
        if (raiz && id.HasValue) return BadRequest();
        if (raiz && !User.IsInRole("Administrador") && !User.IsInRole("Contador")) return Forbid();
        EsRaiz = raiz;
        var token = User.FindFirst("Token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) return RedirectToPage("/Seguridad/Login");
        try
        {
            var actual = id is int valor ? await servicio.ObtenerDetalleAsync(valor, token, ct) : null;
            RequierePrincipal = actual?.IdCuentaPrincipal is not null;
            if (actual is not null) Datos.Codigo = actual.Codigo;
            if (!guardar && actual is not null)
                Datos = new CuentaDatos
                {
                    Codigo = actual.Codigo,
                    Nombre = actual.Nombre,
                    IdCuentaPrincipal = actual.IdCuentaPrincipal,
                    ConciliacionPagos = actual.ConciliacionPagos,
                    IdEstado = actual.IdEstado,
                    Impuestos = actual.Impuestos,
                    Descripcion = actual.Descripcion
                };

            var cuentas = EsRaiz ? new List<CuentaFila>() : await servicio.ObtenerAsync(token, ct);
            Principales = cuentas.Where(c => c.IdCuentaContable != id &&
                (c.IdCuentaContable == actual?.IdCuentaPrincipal ||
                (c.IdEstado == 1 && (actual is null ||
                    (string.Equals(c.TipoCuenta, actual.TipoCuenta, StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(c.Naturaleza, actual.Naturaleza, StringComparison.OrdinalIgnoreCase))))))
                .Select(c => new SelectListItem($"{c.Codigo} — {c.Nombre}", c.IdCuentaContable.ToString()))
                .ToList();
            PuedeGuardar = EsRaiz || id.HasValue || Principales.Count > 0;
            if (!guardar) return Page();

            if (actual is null)
            {
                if (string.IsNullOrWhiteSpace(Datos.Codigo))
                    ModelState.AddModelError("Datos.Codigo", "Escribí el código de la cuenta.");
                if (Datos.IdEstado is not (1 or 2))
                    ModelState.AddModelError("Datos.IdEstado", "Seleccioná el estado inicial.");
            }
            if (!EsRaiz && Datos.IdCuentaPrincipal is null && (actual is null || actual.IdCuentaPrincipal.HasValue))
                ModelState.AddModelError("Datos.IdCuentaPrincipal", "Seleccioná una cuenta principal.");

            if (EsRaiz)
            {
                if (Datos.TipoCuenta is not ("Activo" or "Pasivo" or "Patrimonio" or "Ingreso" or "Gasto"))
                    ModelState.AddModelError("Datos.TipoCuenta", "Seleccioná el tipo de cuenta.");
                if (Datos.Naturaleza is not ("Deudora" or "Acreedora"))
                    ModelState.AddModelError("Datos.Naturaleza", "Seleccioná la naturaleza.");
            }
            if (!ModelState.IsValid || !PuedeGuardar) return Page();

            var datosEditar = new
            {
                Datos.Nombre,
                Datos.IdCuentaPrincipal,
                Datos.ConciliacionPagos,
                Datos.Impuestos,
                Datos.Descripcion
            };
            object datos = id.HasValue ? datosEditar : new
            {
                Datos.Codigo,
                Datos.Nombre,
                Datos.IdCuentaPrincipal,
                Datos.ConciliacionPagos,
                Datos.IdEstado,
                Datos.Impuestos,
                Datos.Descripcion
            };

            var error = EsRaiz
                ? await servicio.AgregarRaizAsync(new
                {
                    Datos.Codigo,
                    Datos.Nombre,
                    Datos.TipoCuenta,
                    Datos.Naturaleza,
                    Datos.ConciliacionPagos,
                    Datos.IdEstado,
                    Datos.Impuestos,
                    Datos.Descripcion
                }, token, ct)
                : await servicio.GuardarAsync(id, datos, token, ct);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                return Page();
            }
            if (EsRaiz)
            {
                TempData["MensajeCuenta"] = "Cuenta raíz creada con éxito.";
                return RedirectToPage("./Listado_CuentaContable");
            }

            TempData["MensajeExito"] = id.HasValue ? "Cuenta contable editada con éxito."
                : "Cuenta contable creada con éxito.";
            return RedirectToPage("./Index");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        { return RedirectToPage("/Seguridad/Login"); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        { return Forbid(); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound && id.HasValue)
        { return NotFound(); }
        catch (Exception ex) when (EsErrorDeCarga(ex, ct))
        { ModelState.AddModelError(string.Empty, "No fue posible completar la solicitud. Intentá nuevamente."); }
        return Page();
    }

    public class CuentaDatos : IValidatableObject
    {
        [StringLength(50)] public string? Codigo { get; set; }
        [Required, StringLength(150)] public string? Nombre { get; set; }
        [Range(1, int.MaxValue)] public int? IdCuentaPrincipal { get; set; }
        [Required] public bool? ConciliacionPagos { get; set; }
        public int? IdEstado { get; set; } = 1;
        public string? TipoCuenta { get; set; }
        public string? Naturaleza { get; set; }
        public decimal? Impuestos { get; set; }
        [StringLength(500)] public string? Descripcion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Impuestos is decimal v && (v < -99999.9999m || v > 99999.9999m || decimal.Round(v, 4) != v))
                yield return new ValidationResult("Ingresá hasta cinco dígitos enteros y cuatro decimales.",
                    new[] { nameof(Impuestos) });
        }
    }
}