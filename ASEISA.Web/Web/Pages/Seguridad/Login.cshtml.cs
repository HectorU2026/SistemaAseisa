using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Abstracciones.Modelos.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.Seguridad
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        [BindProperty]
        public Login loginInfo { get; set; } = default!;
        [BindProperty]
        public Token token { get; set; } = default!;
        public LoginModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var Hash = Autenticacion.GenerarHash(loginInfo.contrasena);
                var contrasena_hash= Autenticacion.ObtenerHash(Hash);

                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "Login");
                var client = new HttpClient();
                var respuesta = await client.PostAsJsonAsync<Login>(endpoint,
                    new Login
                    {
                        Correo = loginInfo.Correo,
                        contrasena = contrasena_hash
                    });
                if (!respuesta.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Hubo un error al intentar iniciar sesión");
                    return Page();
                }

                await ActualizarUltimoAcceso(loginInfo.Correo);

                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                token = JsonSerializer.Deserialize<Token>(
                    respuesta.Content.ReadAsStringAsync().Result, opciones);

                if (token.ValidacionExitosa)
                {
                    JwtSecurityToken? jwtToken = Autenticacion.leerToken(token.AccessToken);
                    var claims = Autenticacion.GenerarClaims(jwtToken, token.AccessToken);
                    await establecerAutenticacion(claims);

                    var urlredirigir = $"{HttpContext.Request.Query["ReturnUrl"]}";
                    if (string.IsNullOrEmpty(urlredirigir))
                        return Redirect("/");
                    return Redirect(urlredirigir);
                }
            }
            return Page();
        }

        private async Task establecerAutenticacion(List<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
        }

        private async Task ActualizarUltimoAcceso(string correoElectronico)
        {
            var endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ActualizarUltimoAcceso");
            var client = new HttpClient();
            var url = $"{endpoint}?correo={Uri.EscapeDataString(correoElectronico)}";
            var respuesta = await client.PostAsync(url, null);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error {respuesta.StatusCode}: {error}");
            }
        }
    }
}
