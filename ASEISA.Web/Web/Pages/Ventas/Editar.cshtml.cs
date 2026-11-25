using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Ventas
{
    public class EditarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public CotizacionRequest Cotizacion { get; set; } = new();

        public List<ContactoResponse> Clientes { get; set; } = new();
        public List<ProductoCatalogoItem> Productos { get; set; } = new();
        public string? MensajeError { get; set; }

        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync(int idCotizacion)
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            var cliente = ObtenerClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            string endpointDetalle = $"{_configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCotizacionPorId")}/{idCotizacion}";
            var respCotizacion = await cliente.GetAsync(endpointDetalle);

            if (!respCotizacion.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró la cotización solicitada.";
                return RedirectToPage("/Ventas/Ventas");
            }

            var jsonCot = await respCotizacion.Content.ReadAsStringAsync();
            var detallada = JsonSerializer.Deserialize<CotizacionResponseDetallada>(jsonCot, opciones);

            if (detallada == null)
            {
                TempData["Error"] = "No se pudo leer la información de la cotización.";
                return RedirectToPage("/Ventas/Ventas");
            }

            // VEN-003: Solo permitir edición si está en estado "Cotización" (IdEstado == 1)
            if (detallada.IdEstado != 1 && !string.Equals(detallada.NombreEstado, "Cotización", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Solo se pueden editar cotizaciones que se encuentren en estado 'Cotización'.";
                return RedirectToPage("/Ventas/Ventas");
            }

            Cotizacion = new CotizacionRequest
            {
                IdCotizacion = detallada.IdCotizacion,
                IdEmpresa = detallada.IdEmpresa,
                IdCliente = detallada.IdCliente,
                NumeroCotizacion = detallada.NumeroCotizacion,
                FechaEmision = detallada.FechaEmision,
                FechaVencimiento = detallada.FechaVencimiento,
                Moneda = string.IsNullOrWhiteSpace(detallada.Moneda) ? "CRC" : detallada.Moneda,
                TipoCambio = detallada.TipoCambio ?? 1.00m,
                Subtotal = detallada.Subtotal,
                Impuesto = detallada.Impuesto,
                Descuento = detallada.Descuento,
                Total = detallada.Total,
                IdEstado = detallada.IdEstado,
                Detalles = detallada.Detalles.Select(d => new DetalleCotizacionRequest
                {
                    IdDetalleCotizacion = d.IdDetalleCotizacion,
                    IdCotizacion = d.IdCotizacion,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    PorcentajeDescuento = d.PorcentajeDescuento,
                    PorcentajeImpuesto = d.PorcentajeImpuesto,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            await CargarCatalogosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            var detallesValidos = (Cotizacion.Detalles ?? new())
                .Where(d => d.IdProducto > 0 && d.Cantidad > 0)
                .ToList();

            if (Cotizacion.IdCotizacion <= 0
                || Cotizacion.IdCliente <= 0
                || string.IsNullOrWhiteSpace(Cotizacion.NumeroCotizacion)
                || !detallesValidos.Any())
            {
                MensajeError = "Complete todos los datos obligatorios y mantenga al menos un producto válido.";
                await CargarCatalogosAsync();
                return Page();
            }

            decimal subtotalBruto = 0m;
            decimal totalDescuento = 0m;
            decimal totalImpuesto = 0m;

            foreach (var d in detallesValidos)
            {
                d.IdCotizacion = Cotizacion.IdCotizacion;
                decimal bruto = d.Cantidad * d.PrecioUnitario;
                decimal desc = bruto * ((d.PorcentajeDescuento ?? 0m) / 100m);
                decimal neto = bruto - desc;
                decimal imp = neto * ((d.PorcentajeImpuesto ?? 0m) / 100m);

                d.Subtotal = Math.Round(neto, 2);
                subtotalBruto += bruto;
                totalDescuento += desc;
                totalImpuesto += imp;
            }

            Cotizacion.IdEmpresa = ObtenerIdEmpresa();
            Cotizacion.IdEstado = 1;
            Cotizacion.Moneda = string.IsNullOrWhiteSpace(Cotizacion.Moneda) ? "CRC" : Cotizacion.Moneda;
            Cotizacion.TipoCambio ??= 1.00m;
            Cotizacion.Subtotal = Math.Round(subtotalBruto, 2);
            Cotizacion.Descuento = Math.Round(totalDescuento, 2);
            Cotizacion.Impuesto = Math.Round(totalImpuesto, 2);
            Cotizacion.Total = Math.Round(subtotalBruto - totalDescuento + totalImpuesto, 2);
            Cotizacion.Detalles = detallesValidos;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "EditarCotizacionFlujo");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PutAsJsonAsync(endpoint, Cotizacion);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Cotización {Cotizacion.NumeroCotizacion} actualizada con éxito.";
                return RedirectToPage("/Ventas/Ventas");
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "Ocurrió un error al actualizar la cotización."
                    : error.Trim('"');
                await CargarCatalogosAsync();
                return Page();
            }
        }

        private async Task CargarCatalogosAsync()
        {
            var cliente = ObtenerClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            int idEmpresa = ObtenerIdEmpresa();

            string endpointContactos = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerContactos");
            string endpointProductos = $"{_configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerProductos")}?IdEmpresa={idEmpresa}&Pagina=1&TamanoPagina=500";

            var tareaContactos = cliente.GetAsync(endpointContactos);
            var tareaProductos = cliente.GetAsync(endpointProductos);

            await Task.WhenAll(tareaContactos, tareaProductos);

            if (tareaContactos.Result.IsSuccessStatusCode)
            {
                var jsonContactos = await tareaContactos.Result.Content.ReadAsStringAsync();
                var todos = JsonSerializer.Deserialize<List<ContactoResponse>>(jsonContactos, opciones) ?? new();
                Clientes = todos
                    .Where(c => c.IdCliente.HasValue && c.IdCliente.Value > 0 && c.IdEstado == 1)
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }

            if (tareaProductos.Result.IsSuccessStatusCode)
            {
                var jsonProductos = await tareaProductos.Result.Content.ReadAsStringAsync();
                var catalogo = JsonSerializer.Deserialize<ProductoCatalogoRespuesta>(jsonProductos, opciones);
                Productos = (catalogo?.Items ?? Enumerable.Empty<ProductoCatalogoItem>())
                    .Where(p => p.IdEstado == 1)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
        }

        private int ObtenerIdEmpresa()
        {
            var empresaStr = User.FindFirst("IdEmpresa")?.Value
                             ?? User.FindFirst("Empresa")?.Value;
            return int.TryParse(empresaStr, out int idEmpresa) && idEmpresa > 0 ? idEmpresa : 1;
        }

        private HttpClient ObtenerClienteConToken()
        {
            var tokenClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Token");
            var cliente = new HttpClient();
            if (tokenClaim != null)
            {
                cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClaim.Value);
            }
            return cliente;
        }
    }
}