using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Web.Modelos;
using Web.Models;

namespace Web.Pages.Seguridad
{
    public class RegistroModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public RegistroFormulario Formulario { get; set; } = new();

        [ValidateNever]
        public Usuario Usuario { get; set; } = default!;

        [ValidateNever]
        public Cliente Cliente { get; set; } = default!;

        public List<EmpresaResponseNombres> Empresas { get; set; } = new();

        public RegistroResponse? registro { get; set; }

        public RegistroModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGet()
        {
            await CargarEmpresas();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                await CargarEmpresas();
                return Page();
            }

            // Generar automáticamente el nombre de usuario.
            string nombreUsuario = Formulario.GenerarNombreUsuario();

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                ModelState.AddModelError(
                    "Formulario.NombreUsuario",
                    "No fue posible generar el nombre de usuario.");

                await CargarEmpresas();
                return Page();
            }

            // Datos de la persona que accederá al sistema.
            // En personas jurídicas, corresponde al representante.
            Usuario = new Usuario
            {
                IdEmpresa = Formulario.IdEmpresa,
                TipoCliente = Formulario.TipoCliente,
                Nombre = Formulario.Nombre.Trim(),
                PrimerApellido = Formulario.PrimerApellido.Trim(),
                SegundoApellido = Formulario.SegundoApellido?.Trim(),
                Correo = Formulario.Correo.Trim(),
                NombreUsuario = nombreUsuario,

                // Seguridad.API aplicará PBKDF2.
                ContrasenaHash = Formulario.Contrasena,

                IdEstado = 1
            };

            // Datos del cliente, sea persona física o jurídica.
            Cliente = new Cliente
            {
                IdEmpresa = Formulario.IdEmpresa,
                TipoCliente = Formulario.TipoCliente,

                NombreRazonSocial =
                    Formulario.TipoCliente == "Persona jurídica"
                    ? Formulario.RazonSocial!.Trim()
                    : $"{Formulario.Nombre.Trim()} " +
                      $"{Formulario.PrimerApellido.Trim()} " +
                      $"{Formulario.SegundoApellido?.Trim()}".Trim(),

                Identificacion = Formulario.Identificacion,
                Correo = Formulario.Correo.Trim(),
                Telefono = Formulario.Telefono?.Trim(),
                Direccion = Formulario.Direccion?.Trim(),
                IdEstado = 1
            };

            var request = new RegistroRequest
            {
                Usuario = Usuario,
                Cliente = Cliente
            };

            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPointsSeguridad", "Registro");

            using var client = new HttpClient();

            try
            {
                var respuesta = await client.PostAsJsonAsync(
                    endpoint, request);

                var contenido = await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "No fue posible completar el registro. " +
                        "Verifique los datos e inténtelo nuevamente.");

                    await CargarEmpresas();
                    return Page();
                }

                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                registro = JsonSerializer.Deserialize<RegistroResponse>(
                    contenido, opciones);

                if (registro?.IdUsuario == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        registro?.mensaje ??
                        "No fue posible confirmar el registro.");

                    await CargarEmpresas();
                    return Page();
                }

                Usuario.IdUsuario = registro.IdUsuario;

                // Conservar la bitácora del registro.
                await RegistrarBitacora(Usuario, Cliente);

                return RedirectToPage("/Seguridad/Login");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible conectar con el servicio de registro.");

                await CargarEmpresas();
                return Page();
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El servicio devolvió una respuesta no válida.");

                await CargarEmpresas();
                return Page();
            }
        }

        public async Task CargarEmpresas()
        {
            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints", "ObtenerNombreEmpresa");

            using var cliente = new HttpClient();

            var respuesta = await cliente.GetAsync(endpoint);
            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content.ReadAsStringAsync();

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Empresas =
                JsonSerializer.Deserialize<List<EmpresaResponseNombres>>(
                    resultado, opciones) ?? new();
        }

        private async Task RegistrarBitacora(
            Usuario usuario, Cliente cliente)
        {
            var bitacora = new Bitacora
            {
                IdUsuario = usuario.IdUsuario!.Value,
                IdEmpresa = usuario.IdEmpresa!.Value,
                TablaAfectada = "Usuario, Cliente",
                Accion = "RegistrarUsuarioCliente",
                RegistroId = usuario.IdUsuario.ToString(),
                ValorAnterior = null,

                ValorNuevo = JsonSerializer.Serialize(new
                {
                    usuario = new
                    {
                        usuario.IdEmpresa,
                        usuario.Nombre,
                        usuario.PrimerApellido,
                        usuario.SegundoApellido,
                        usuario.Correo,
                        usuario.NombreUsuario,
                        usuario.IdEstado
                    },

                    cliente = new
                    {
                        cliente.IdEmpresa,
                        cliente.TipoCliente,
                        cliente.NombreRazonSocial,
                        cliente.Identificacion,
                        cliente.Correo,
                        cliente.Telefono,
                        cliente.Direccion,
                        cliente.IdEstado
                    },

                    fecha = DateTime.UtcNow
                })
            };

            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints", "RegistrarBitacora");

            using var client = new HttpClient();

            var respuesta = await client.PostAsJsonAsync(
                endpoint, bitacora);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error bitácora: {error}");
            }
        }
    }
}
