
using Abstracciones.Reglas;
using Abstracciones.DA;
using Abstracciones.Modelos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Seguridad.Criptografia;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BC
{
    public class AutenticacionReglas : IAutenticacionBC
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioDA _usuarioDA;

        public AutenticacionReglas(
            IConfiguration configuration,
            IUsuarioDA usuarioDA)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
        }

        public async Task<Token> LoginAync(LoginBase login)
        {
            var resultado = new Token
            {
                AccessToken = string.Empty,
                ValidacionExitosa = false
            };

            if (login == null ||
                string.IsNullOrWhiteSpace(login.Correo) ||
                string.IsNullOrWhiteSpace(login.ContrasenaHash))
            {
                return resultado;
            }

            var filasUsuario = (await _usuarioDA.ObtenerUsuarioConRoles(login))
                .ToList();

            var usuario = filasUsuario.FirstOrDefault();

            if (usuario == null)
                return resultado;

            bool credencialesValidas = ContrasenasSeguras.Verificar(
                login.ContrasenaHash,
                usuario.ContrasenaHash,
                out bool requiereMigracion);

            if (!credencialesValidas)
                return resultado;

            if (requiereMigracion)
            {
                await _usuarioDA.CambiarContrasena(
                    new CambiarContrasena
                    {
                        correo = usuario.Correo,
                        ContrasenaHash = ContrasenasSeguras.Crear(
                            login.ContrasenaHash),
                        Token = string.Empty
                    });
            }

            var cfg = _configuration
                .GetSection("Token")
                .Get<TokenConfiguracion>()
                ?? throw new InvalidOperationException(
                    "Falta la configuración Token.");

            var llave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(cfg.key));

            var credenciales = new SigningCredentials(
                llave,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo)
            };

            var rolesActivos = filasUsuario
                .Select(fila => fila.rol)
                .Where(rol => !string.IsNullOrWhiteSpace(rol))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var rol in rolesActivos)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var jwt = new JwtSecurityToken(
                cfg.Issuer,
                cfg.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(cfg.Expires),
                signingCredentials: credenciales);

            resultado.AccessToken =
                new JwtSecurityTokenHandler().WriteToken(jwt);

            resultado.ValidacionExitosa = true;

            return resultado;
        }
    }
}
