using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.Seguridad
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public Login loginInfo { get; set; } = new Login
        {
            Correo = "",
            ContrasenaHash = ""
        };

        public string? MensajeError { get; private set; }

        public LoginModel(
            IConfiguracion configuracion,
            IHttpClientFactory httpClientFactory)
        {
            _configuracion = configuracion;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            NormalizarErrores();
            if (!ModelState.IsValid) return Page();
            var correo = loginInfo.Correo.Trim();
            try
            {
                var endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "Login");
                using var respuesta = await _httpClientFactory.CreateClient().PostAsJsonAsync(
                    endpoint, new Login { Correo = correo, ContrasenaHash = loginInfo.ContrasenaHash });
                if (!respuesta.IsSuccessStatusCode)
                {
                    MensajeError = "No fue posible iniciar sesión. Verifique sus credenciales.";
                    return Page();
                }
                var token = await respuesta.Content.ReadFromJsonAsync<Token>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (token?.ValidacionExitosa != true || string.IsNullOrWhiteSpace(token.AccessToken))
                {
                    MensajeError = "No fue posible validar su sesión. Verifique sus credenciales.";
                    return Page();
                }
                var jwt = Autenticacion.leerToken(token.AccessToken);
                if (jwt == null) throw new JsonException("Token inválido.");
                var claims = Autenticacion.GenerarClaims(jwt, token.AccessToken);
                var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identidad));
                await EjecutarSinBloquearLogin(() => ActualizarUltimoAcceso(correo));
                await EjecutarSinBloquearLogin(() => RegistrarBitacora(correo));
                var returnUrl = Request.Query["ReturnUrl"].ToString();
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl) : RedirectToPage("/Index");
            }
            catch (HttpRequestException)
            {
                MensajeError = "No fue posible conectar con el servidor de seguridad.";
            }
            catch (TaskCanceledException)
            {
                MensajeError = "La conexión tardó demasiado. Intente nuevamente.";
            }
            catch (JsonException)
            {
                MensajeError = "El servidor devolvió una respuesta inesperada.";
            }
            catch (Exception)
            {
                MensajeError = "Ocurrió un error inesperado al iniciar sesión.";
            }
            return Page();
        }

        private void NormalizarErrores()
        {
            var correoVacio =
                string.IsNullOrWhiteSpace(loginInfo?.Correo);

            var claveVacia =
                string.IsNullOrWhiteSpace(loginInfo?.ContrasenaHash);

            ModelState.Clear();

            if (correoVacio)
            {
                ModelState.AddModelError(
                    "loginInfo.Correo",
                    "Ingrese su correo electrónico."
                );
            }
            else if (!new EmailAddressAttribute().IsValid(loginInfo.Correo))
            {
                ModelState.AddModelError(
                    "loginInfo.Correo",
                    "Ingrese un correo electrónico válido."
                );
            }

            if (claveVacia)
            {
                ModelState.AddModelError(
                    "loginInfo.ContrasenaHash",
                    "Ingrese su contraseña."
                );
            }
        }

        private async Task EjecutarSinBloquearLogin(
            Func<Task> accion)
        {
            try
            {
                await accion();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task ActualizarUltimoAcceso(
            string correo)
        {
            var endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ActualizarUltimoAcceso"
            );

            var url =
                $"{endpoint}?correo={Uri.EscapeDataString(correo)}";

            using var respuesta =
                await _httpClientFactory
                    .CreateClient()
                    .PostAsync(url, null);

            respuesta.EnsureSuccessStatusCode();
        }

        private async Task RegistrarBitacora(
            string correo)
        {
            var usuario =
                await ObtenerInformacionUsuario(correo);

            if (usuario == null)
                return;

            var bitacora = new
            {
                IdUsuario = usuario.IdUsuario,
                IdEmpresa = usuario.IdEmpresa,
                TablaAfectada = "Usuario",
                RegistroId = usuario.IdUsuario?.ToString(),
                Accion = "Login",
                ValorAnterior = (string?)null,

                ValorNuevo = JsonSerializer.Serialize(
                    new
                    {
                        usuario.IdUsuario,
                        usuario.IdEmpresa,
                        usuario.Nombre,
                        usuario.PrimerApellido,
                        usuario.SegundoApellido,
                        usuario.Correo,
                        ContrasenaHash = "***",
                        usuario.NombreUsuario,
                        usuario.UltimoAcceso,
                        usuario.IdEstado,
                        usuario.FechaCreacion,
                        usuario.FechaModificacion,
                        fecha = DateTime.UtcNow
                    }
                )
            };

            var endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "RegistrarBitacora"
            );

            using var respuesta =
                await _httpClientFactory
                    .CreateClient()
                    .PostAsJsonAsync(endpoint, bitacora);

            respuesta.EnsureSuccessStatusCode();
        }

        private async Task<Usuario?> ObtenerInformacionUsuario(
            string correo)
        {
            var endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerInfoUsuario"
            );

            var url =
                $"{endpoint}?correo={Uri.EscapeDataString(correo)}";

            using var respuesta =
                await _httpClientFactory
                    .CreateClient()
                    .PostAsync(url, null);

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadFromJsonAsync<Usuario>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
        }
    }
}
