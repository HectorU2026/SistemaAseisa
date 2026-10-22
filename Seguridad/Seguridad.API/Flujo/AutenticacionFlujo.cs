using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Seguridad.Criptografia;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Flujo
{
    public class AutenticacionFlujo : IAutenticacionFlujo
    {

        private IAutenticacionBC _autenticacionBC;
        private IRecuperarContrasena _recuperarContrasena;
        private IUsuarioDA _usuarioDA;
        private readonly IConfiguration _configuration;

        public AutenticacionFlujo(IAutenticacionBC autenticacionBC, IRecuperarContrasena recuperarContrasena, IUsuarioDA usuario, IConfiguration configuration)
        {
            _recuperarContrasena = recuperarContrasena;
            _autenticacionBC = autenticacionBC;
            _usuarioDA = usuario;
            _configuration = configuration;
        }

        public async Task<Token> LoginAsync(LoginBase login)
        {
            return await _autenticacionBC.LoginAync(login);
        }

        public async Task<IActionResult> RecuperarContrasena(UsuarioRecuperar correo)
        {
            return await _recuperarContrasena.GestionRecuperarContrasena(correo);
        }

        public async Task<IActionResult> CambiarContrasena(CambiarContrasena usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Token) ||
                string.IsNullOrWhiteSpace(usuario.ContrasenaHash))
                return new BadRequestObjectResult("Solicitud inválida.");

            var cfg = _configuration.GetSection("Token").Get<TokenConfiguracion>();
            if (cfg == null) return new BadRequestObjectResult("Configuración inválida.");
            try
            {
                var parametros = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = cfg.Issuer,
                    ValidateAudience = true,
                    ValidAudience = cfg.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.key)),
                    ClockSkew = TimeSpan.Zero
                };
                var principal = new JwtSecurityTokenHandler().ValidateToken(usuario.Token, parametros, out _);
                var correoToken = principal.FindFirst("correo")?.Value;
                if (string.IsNullOrWhiteSpace(correoToken))
                    return new BadRequestObjectResult("Token de recuperación inválido.");
                var activo = await _usuarioDA.ValidarCorreoActivo(correoToken);
                if (activo == null) return new BadRequestObjectResult("Cuenta no disponible.");
                usuario.correo = correoToken; 
                usuario.ContrasenaHash = ContrasenasSeguras.Crear(usuario.ContrasenaHash);
                return await _usuarioDA.CambiarContrasena(usuario);
            }
            catch (SecurityTokenException)
            {
                return new BadRequestObjectResult("Token de recuperación inválido o vencido.");
            }
            catch (ArgumentException)
            {
                return new BadRequestObjectResult("Token de recuperación inválido.");
            }
        }
    }
}
