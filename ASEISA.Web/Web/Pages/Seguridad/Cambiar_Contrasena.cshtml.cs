using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Web.Views.Home.Inicio_Sesion
{
    public class Cambiar_ContrasenaModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public CambiarContrasenaRequest Cambio { get; set; } = default!;

        public Cambiar_ContrasenaModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Redirect("/Seguridad/Login");

            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (Cambio.NuevaContrasena != Cambio.ConfirmarContrasena)
                {
                    ModelState.AddModelError("", "Las contraseñas no coinciden");
                    CargarToken(Cambio.Token);
                    return Page();
                }

                var correo = ObtenerCorreoDelToken(Cambio.Token);
                if (string.IsNullOrEmpty(correo))
                {
                    ModelState.AddModelError("", "Token invalido");
                    return Page();
                }

                var usuarioAntes = await ObtenerInformacionUsuario(correo);
                if (usuarioAntes == null)
                {
                    return Page();
                }

                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "CambiarContrasena");
                var client = new HttpClient();
                var Hash = Autenticacion.GenerarHash(Cambio.NuevaContrasena);
                var contrasena_hash = Autenticacion.ObtenerHash(Hash);
                var respuesta = await client.PostAsJsonAsync(endpoint,
                    new
                    {
                        Token = Cambio.Token,
                        correo = correo,
                        ContrasenaHash = contrasena_hash
                    });

                if (!respuesta.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Error al cambiar la contraseña");
                    CargarToken(Cambio.Token);
                    return Page();
                }

                await RegistrarBitacora(usuarioAntes, contrasena_hash);
                return Redirect("/Seguridad/Login");
            }
            CargarToken(Cambio.Token);
            return Page();
        }

        private string? ObtenerCorreoDelToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "correo")?.Value;
        }

        private async Task RegistrarBitacora(Usuario usuarioAntes, string contrasena_hash)
        {
            var usuarioDespues = new Usuario
            {
                IdUsuario = usuarioAntes.IdUsuario,
                IdEmpresa = usuarioAntes.IdEmpresa,
                Nombre = usuarioAntes.Nombre,
                TipoCliente = usuarioAntes.TipoCliente,
                PrimerApellido = usuarioAntes.PrimerApellido,
                SegundoApellido = usuarioAntes.SegundoApellido,
                Correo = usuarioAntes.Correo,
                ContrasenaHash = usuarioAntes.ContrasenaHash,
                NombreUsuario = usuarioAntes.NombreUsuario,
                UltimoAcceso = usuarioAntes.UltimoAcceso,
                IdEstado = usuarioAntes.IdEstado,
                FechaCreacion = usuarioAntes.FechaCreacion,
                FechaModificacion = usuarioAntes.FechaModificacion,
            };

            var bitacora = new
            {
                IdUsuario = usuarioAntes.IdUsuario,
                IdEmpresa = usuarioAntes.IdEmpresa,
                TablaAfectada = "Usuario",
                RegistroId = usuarioAntes.IdUsuario?.ToString(),
                Accion = "CambiarContrasena",
                ValorAnterior = JsonSerializer.Serialize(new
                {
                    usuarioAntes.IdUsuario,
                    usuarioAntes.IdEmpresa,
                    usuarioAntes.Nombre,
                    usuarioAntes.TipoCliente,
                    usuarioAntes.PrimerApellido,
                    usuarioAntes.SegundoApellido,
                    usuarioAntes.Correo,
                    ContrasenaHash = "***",
                    usuarioAntes.NombreUsuario,
                    usuarioAntes.UltimoAcceso,
                    usuarioAntes.IdEstado,
                    usuarioAntes.FechaCreacion,
                    usuarioAntes.FechaModificacion,
                    fecha = DateTime.UtcNow
                }),
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    usuarioDespues.IdUsuario,
                    usuarioDespues.IdEmpresa,
                    usuarioDespues.Nombre,
                    usuarioDespues.TipoCliente,
                    usuarioDespues.PrimerApellido,
                    usuarioDespues.SegundoApellido,
                    usuarioDespues.Correo,
                    ContrasenaHash = "***",
                    usuarioDespues.NombreUsuario,
                    usuarioDespues.UltimoAcceso,
                    usuarioDespues.IdEstado,
                    usuarioDespues.FechaCreacion,
                    FechaModificacion = DateTime.UtcNow,
                    fecha = DateTime.UtcNow
                })
            };

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarBitacora");
            var client = new HttpClient();
            var respuesta = await client.PostAsJsonAsync(endpoint, bitacora);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error bitacora: {error}");
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

        private void CargarToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                Token = token;
        }

    }
}
