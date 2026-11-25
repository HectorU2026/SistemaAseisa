using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Ventas
{
    public class CrearModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public CotizacionRequest Cotizacion { get; set; } = new();

        public List<ContactoResponse> Clientes { get; set; } = new();
        public List<ProductoCatalogoItem> Productos { get; set; } = new();
        public string? MensajeError { get; set; }

        public CrearModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.FindFirst("Empresa") is null)
                return RedirectToPage("/Seguridad/Login");

            Cotizacion.NumeroCotizacion = $"COT-{DateTime.Now:yyyyMMdd-HHmm}";
            Cotizacion.FechaEmision = DateTime.Today;
            Cotizacion.FechaVencimiento = DateTime.Today.AddDays(15);
            Cotizacion.Moneda = "CRC";
            Cotizacion.TipoCambio = 1.00m;

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

            if (Cotizacion.IdCliente <= 0
                || string.IsNullOrWhiteSpace(Cotizacion.NumeroCotizacion)
                || !detallesValidos.Any())
            {
                MensajeError = "Seleccione un cliente, indique el número de cotización y agregue al menos un producto válido.";
                await CargarCatalogosAsync();
                return Page();
            }

            // Recalcular subtotales e impuestos de forma exacta antes de enviar a la API
            decimal subtotalBruto = 0m;
            decimal totalDescuento = 0m;
            decimal totalImpuesto = 0m;

            foreach (var d in detallesValidos)
            {
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
            Cotizacion.IdEstado = 1; // Estado inicial: Cotización
            Cotizacion.Moneda = string.IsNullOrWhiteSpace(Cotizacion.Moneda) ? "CRC" : Cotizacion.Moneda;
            Cotizacion.TipoCambio ??= 1.00m;
            Cotizacion.Subtotal = Math.Round(subtotalBruto, 2);
            Cotizacion.Descuento = Math.Round(totalDescuento, 2);
            Cotizacion.Impuesto = Math.Round(totalImpuesto, 2);
            Cotizacion.Total = Math.Round(subtotalBruto - totalDescuento + totalImpuesto, 2);
            Cotizacion.Detalles = detallesValidos;

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarCotizacionFlujo");
            var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.PostAsJsonAsync(endpoint, Cotizacion);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Cotización {Cotizacion.NumeroCotizacion} creada con éxito.";
                return RedirectToPage("/Ventas/Ventas");
            }
            else
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(error)
                    ? "Ocurrió un error al registrar la cotización."
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