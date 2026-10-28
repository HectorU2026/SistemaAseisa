using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Web.Servicios;

namespace Web.Pages.Contacto
{
    public class Editar_ContactoModel : PageModel
    {
        private readonly ContactoApiService _contactoApiService;

        public Editar_ContactoModel(
            ContactoApiService contactoApiService)
        {
            _contactoApiService = contactoApiService;
        }

        [BindProperty]
        public ContactoRequest Contacto { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int IdContacto { get; set; }

        public ContactoCatalogosPagoResponse Catalogos { get; set; } = new();

        public bool CatalogosCargados { get; private set; }

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string? token = ObtenerToken();

            if (token == null)
                return RedirectToPage("/Seguridad/Login");

            if (IdContacto <= 0)
                return RedirectToPage("/Contacto/Listado_Contacto");

            try
            {
                var contacto = await _contactoApiService
                    .ObtenerContactoPorIdAsync(IdContacto, token);

                if (contacto == null)
                {
                    MensajeError = "El contacto solicitado no existe.";
                    return Page();
                }

                Contacto = ConvertirARequest(contacto);

                await CargarCatalogosAsync(token);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (Exception)
            {
                MensajeError =
                    "No fue posible cargar la información del contacto.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? token = ObtenerToken();

            if (token == null)
                return RedirectToPage("/Seguridad/Login");

            if (IdContacto <= 0)
                return BadRequest("El identificador del contacto no es válido.");

            ContactoResponse? original;

            try
            {
                original = await _contactoApiService
                    .ObtenerContactoPorIdAsync(IdContacto, token);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Seguridad/Login");
            }
            catch (Exception)
            {
                MensajeError =
                    "No fue posible verificar la información del contacto.";
                return Page();
            }

            if (original == null)
                return NotFound();

            Contacto.IdEstado = original.IdEstado;

            ModelState.Remove("Contacto.IdEstado");

            NormalizarConfiguracionPorRol();

            await CargarCatalogosAsync(token);

            if (!CatalogosCargados)
                return Page();

            ValidarCatalogosSeleccionados();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _contactoApiService
                    .EditarContactoAsync(IdContacto, Contacto, token);

                TempData["MensajeExito"] =
                    $"El contacto #{IdContacto} se actualizó correctamente.";

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
                        "La cédula o identificación fiscal ingresada ya está registrada en otro contacto.",

                    HttpStatusCode.NotFound =>
                        "El contacto ya no existe o no está disponible.",

                    _ =>
                        "No fue posible actualizar el contacto. Verifique la conexión con la API."
                };
            }
            catch (Exception)
            {
                MensajeError =
                    "Ocurrió un error inesperado al actualizar el contacto.";
            }

            return Page();
        }

        private string? ObtenerToken()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            string? token = User.FindFirst("Token")?.Value;

            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        private static ContactoRequest ConvertirARequest(
            ContactoResponse origen)
        {
            return new ContactoRequest
            {
                Cedula = origen.Cedula,
                IdentificacionFiscal = origen.IdentificacionFiscal,
                Nombre = origen.Nombre,
                Correo = origen.Correo,
                Telefono = origen.Telefono,
                Pais = origen.Pais,
                Provincia = origen.Provincia,
                Canton = origen.Canton,
                Distrito = origen.Distrito,
                DetalleDireccion = origen.DetalleDireccion,
                RolContacto = origen.RolContacto,
                IdEstado = origen.IdEstado,
                IdVendedor = origen.IdVendedor,
                IdComprador = origen.IdComprador,
                IdTerminoPagoVentas = origen.IdTerminoPagoVentas,
                IdMetodoPagoVentas = origen.IdMetodoPagoVentas,
                IdTerminoPagoCompras = origen.IdTerminoPagoCompras,
                IdMetodoPagoCompras = origen.IdMetodoPagoCompras
            };
        }

        private void NormalizarConfiguracionPorRol()
        {
            if (Contacto.RolContacto == RolesContacto.Cliente)
            {
                Contacto.IdComprador = null;
                Contacto.IdTerminoPagoCompras = null;
                Contacto.IdMetodoPagoCompras = null;

                ModelState.Remove("Contacto.IdComprador");
                ModelState.Remove("Contacto.IdTerminoPagoCompras");
                ModelState.Remove("Contacto.IdMetodoPagoCompras");
            }
            else if (Contacto.RolContacto == RolesContacto.Proveedor)
            {
                Contacto.IdVendedor = null;
                Contacto.IdTerminoPagoVentas = null;
                Contacto.IdMetodoPagoVentas = null;

                ModelState.Remove("Contacto.IdVendedor");
                ModelState.Remove("Contacto.IdTerminoPagoVentas");
                ModelState.Remove("Contacto.IdMetodoPagoVentas");
            }
        }

        private void ValidarCatalogosSeleccionados()
        {
            var idsTerminos = Catalogos.TerminosPago
                .Select(x => x.IdTerminoPago)
                .ToHashSet();

            var idsMetodos = Catalogos.MetodosPago
                .Select(x => x.IdMetodoPago)
                .ToHashSet();

            ValidarSeleccion(
                Contacto.IdTerminoPagoVentas,
                idsTerminos,
                "Contacto.IdTerminoPagoVentas",
                "Seleccione un término de pago de ventas válido.");

            ValidarSeleccion(
                Contacto.IdTerminoPagoCompras,
                idsTerminos,
                "Contacto.IdTerminoPagoCompras",
                "Seleccione un término de pago de compras válido.");

            ValidarSeleccion(
                Contacto.IdMetodoPagoVentas,
                idsMetodos,
                "Contacto.IdMetodoPagoVentas",
                "Seleccione un método de pago de ventas válido.");

            ValidarSeleccion(
                Contacto.IdMetodoPagoCompras,
                idsMetodos,
                "Contacto.IdMetodoPagoCompras",
                "Seleccione un método de pago de compras válido.");
        }

        private void ValidarSeleccion(
            int? valor,
            HashSet<int> idsPermitidos,
            string campo,
            string mensaje)
        {
            if (valor.HasValue && !idsPermitidos.Contains(valor.Value))
                ModelState.AddModelError(campo, mensaje);
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
