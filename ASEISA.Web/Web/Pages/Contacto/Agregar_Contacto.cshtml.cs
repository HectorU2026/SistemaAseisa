
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Contacto
{
    public class Agregar_ContactoModel : PageModel
    {
        private readonly ContactoApiService _contactoApiService;

        public Agregar_ContactoModel(
            ContactoApiService contactoApiService)
        {
            _contactoApiService = contactoApiService;
        }

        [BindProperty]
        public ContactoRequest Contacto { get; set; } = new()
        {
            Pais = "Costa Rica",
            RolContacto = RolesContacto.Cliente,
            IdEstado = 1
        };

        public ContactoCatalogosPagoResponse Catalogos { get; set; } = new();

        public string? MensajeError { get; set; }

        public bool CatalogosCargados { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Seguridad/Login");

            string? token = User.FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToPage("/Seguridad/Login");

            await CargarCatalogosAsync(token);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Seguridad/Login");

            string? token = User.FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToPage("/Seguridad/Login");

            NormalizarConfiguracionPorRol();

            await CargarCatalogosAsync(token);

            if (!CatalogosCargados)
                return Page();

            RevalidarContacto();

            ValidarCatalogosSeleccionados();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var resultado = await _contactoApiService
                    .AgregarContactoAsync(Contacto, token);

                TempData["MensajeExito"] =
                    $"El contacto #{resultado.IdContacto} se registró correctamente.";

                return RedirectToPage("/Contacto/Listado_Contacto");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException ex)
            {
                MensajeError = ex.StatusCode switch
                {
                    HttpStatusCode.BadRequest =>
                        "La API rechazó los datos. Revise la información ingresada.",

                    HttpStatusCode.Conflict =>
                        "Ya existe un contacto con información que entra en conflicto con este registro.",

                    _ =>
                        "No fue posible registrar el contacto. Verifique la conexión con la API."
                };
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error inesperado al registrar el contacto.";
            }

            return Page();
        }

        private void NormalizarConfiguracionPorRol()
        {
            if (Contacto.RolContacto == RolesContacto.Cliente)
            {
                Contacto.IdComprador = null;
                Contacto.IdTerminoPagoCompras = null;
                Contacto.IdMetodoPagoCompras = null;

                LimpiarValidacionCampo(nameof(Contacto.IdComprador));
                LimpiarValidacionCampo(nameof(Contacto.IdTerminoPagoCompras));
                LimpiarValidacionCampo(nameof(Contacto.IdMetodoPagoCompras));
            }
            else if (Contacto.RolContacto == RolesContacto.Proveedor)
            {
                Contacto.IdVendedor = null;
                Contacto.IdTerminoPagoVentas = null;
                Contacto.IdMetodoPagoVentas = null;

                LimpiarValidacionCampo(nameof(Contacto.IdVendedor));
                LimpiarValidacionCampo(nameof(Contacto.IdTerminoPagoVentas));
                LimpiarValidacionCampo(nameof(Contacto.IdMetodoPagoVentas));
            }
        }

        private void LimpiarValidacionCampo(string nombrePropiedad)
        {
            ModelState.Remove($"Contacto.{nombrePropiedad}");
        }

        private void RevalidarContacto()
        {
            // Conservar los errores producidos durante el model binding.
            // No utilizar ModelState.Clear().

            // Eliminar únicamente los errores de validación anteriores,
            // conservando los errores de conversión.
            foreach (var entrada in ModelState)
            {
                if (entrada.Value.ValidationState ==
                    ModelValidationState.Invalid)
                {
                    // No modificamos las entradas existentes aquí:
                    // TryValidateModel se encargará de validar el objeto.
                }
            }

            TryValidateModel(Contacto, nameof(Contacto));
        }

        private void ValidarCatalogosSeleccionados()
        {
            var idsTerminos = Catalogos.TerminosPago
                .Select(t => t.IdTerminoPago)
                .ToHashSet();

            var idsMetodos = Catalogos.MetodosPago
                .Select(m => m.IdMetodoPago)
                .ToHashSet();

            if (Contacto.IdTerminoPagoVentas.HasValue &&
                !idsTerminos.Contains(Contacto.IdTerminoPagoVentas.Value))
            {
                ModelState.AddModelError(
                    "Contacto.IdTerminoPagoVentas",
                    "Seleccione un término de pago de ventas válido.");
            }

            if (Contacto.IdTerminoPagoCompras.HasValue &&
                !idsTerminos.Contains(Contacto.IdTerminoPagoCompras.Value))
            {
                ModelState.AddModelError(
                    "Contacto.IdTerminoPagoCompras",
                    "Seleccione un término de pago de compras válido.");
            }

            if (Contacto.IdMetodoPagoVentas.HasValue &&
                !idsMetodos.Contains(Contacto.IdMetodoPagoVentas.Value))
            {
                ModelState.AddModelError(
                    "Contacto.IdMetodoPagoVentas",
                    "Seleccione un método de pago de ventas válido.");
            }

            if (Contacto.IdMetodoPagoCompras.HasValue &&
                !idsMetodos.Contains(Contacto.IdMetodoPagoCompras.Value))
            {
                ModelState.AddModelError(
                    "Contacto.IdMetodoPagoCompras",
                    "Seleccione un método de pago de compras válido.");
            }
        }

        private async Task CargarCatalogosAsync(string token)
        {
            CatalogosCargados = false;

            try
            {
                Catalogos = await _contactoApiService
                    .ObtenerCatalogosPagoAsync(token);

                CatalogosCargados = true;
            }
            catch (Exception)
            {
                Catalogos = new ContactoCatalogosPagoResponse();

                MensajeError =
                    "No fue posible cargar los catálogos de pago.";
            }
        }
    }
}
