using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;
using System.Text.Json;

namespace Web.Pages.Seguridad
{
    public class RegistroModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public RegistroModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPost()
        {
            var hash = Autenticacion.GenerarHash(Usuario.ContrasenaHash);
            Usuario.ContrasenaHash = Autenticacion.ObtenerHash(hash);

            Usuario.IdEmpresa = 1;
            Usuario.IdEstado = 1;
            Usuario.TipoCliente = "Persona física";

            Cliente.IdEmpresa = 1;
            Cliente.IdEstado = 1;
            Cliente.TipoCliente = "Persona física";
            Cliente.NombreRazonSocial = $"{Usuario.Nombre} {Usuario.PrimerApellido}";
            if (ModelState.IsValid)
            {
                var request = new
                {
                    Usuario = Usuario,
                    Cliente = Cliente
                };

                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "Registro");
                var client = new HttpClient();
                var respuesta = await client.PostAsJsonAsync(endpoint, request);

                if (respuesta.IsSuccessStatusCode)
                    await RegistrarBitacora(Usuario.Correo);
                    return Redirect("/Seguridad/Login");

            }
            ModelState.AddModelError("", "Asegúrese de rellenar todos los campos obligatorios");
            return Page();
        }

        private async Task RegistrarBitacora(string correo)
        {
            var infoUsuario = await ObtenerInformacionUsuario(correo);
            if (infoUsuario == null) return;

            var bitacora = new
            {
                IdUsuario = infoUsuario.IdUsuario ?? 0,
                IdEmpresa = infoUsuario.IdEmpresa ?? 1,
                TablaAfectada = "Usuario",
                RegistroId = infoUsuario.IdUsuario?.ToString(),
                Accion = "RegistrarUsuario",
                ValorAnterior = (string?)null,
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    infoUsuario.IdUsuario,
                    infoUsuario.IdEmpresa,
                    infoUsuario.Nombre,
                    infoUsuario.PrimerApellido,
                    infoUsuario.SegundoApellido,
                    infoUsuario.Correo,
                    infoUsuario.NombreUsuario,
                    infoUsuario.IdEstado,
                    fecha = DateTime.UtcNow
                })
            };

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarBitacora");
            var client = new HttpClient();
            var respuesta = await client.PostAsJsonAsync(endpoint, bitacora);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error bitácora: {error}");
            }
        }

        private async Task<Usuario?> ObtenerInformacionUsuario(string correoElectronico)
        {
            var endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerInfoUsuario");
            var client = new HttpClient();
            var url = $"{endpoint}?correo={Uri.EscapeDataString(correoElectronico)}";
            var respuesta = await client.PostAsync(url, null);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error {respuesta.StatusCode}: {error}");
            }

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var contenido = await respuesta.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Usuario>(contenido, opciones);
        }
    }
}
