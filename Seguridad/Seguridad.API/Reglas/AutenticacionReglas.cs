using Abstracciones.Reglas;
using Abstracciones.DA;
using Abstracciones.Modelos;
using Seguridad.Criptografia;
using System.Security.Claims;
using System.Text;

namespace BC
{
    public class AutenticacionReglas : IAutenticacionBC
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioDA _usuarioDA;

        public AutenticacionReglas(IConfiguration configuration, IUsuarioDA usuarioDA)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
        }

        public async Task<Token> LoginAync(LoginBase login)
        {
            var resultado = new Token { AccessToken = string.Empty, ValidacionExitosa = false };
            if (login == null || string.IsNullOrWhiteSpace(login.Correo) ||
                string.IsNullOrWhiteSpace(login.ContrasenaHash)) return resultado;

            var usuario = await _usuarioDA.ObtenerUsuario(login);
            if (usuario == null) return resultado;
            if (!ContrasenasSeguras.Verificar(login.ContrasenaHash, usuario.ContrasenaHash,
                out bool requiereMigracion)) return resultado;

            if (requiereMigracion)
            {
                // La migración se realiza solo tras verificar la contraseña anterior.
                await _usuarioDA.CambiarContrasena(new CambiarContrasena
                {
                    correo = usuario.Correo,
                    ContrasenaHash = ContrasenasSeguras.Crear(login.ContrasenaHash),
                    Token = string.Empty
                });
            }

            var cfg = _configuration.GetSection("Token").Get<TokenConfiguracion>()
                ?? throw new InvalidOperationException("Falta la configuración Token.");
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.key));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Role, usuario.rol),
                new(ClaimTypes.Email, usuario.Correo)
            };
            var jwt = new JwtSecurityToken(cfg.Issuer, cfg.Audience, claims,
                expires: DateTime.UtcNow.AddMinutes(cfg.Expires), signingCredentials: credenciales);
            resultado.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            resultado.ValidacionExitosa = true;
            return resultado;
        }
    }
}
