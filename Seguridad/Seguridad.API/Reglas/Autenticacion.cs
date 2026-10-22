using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Reglas
{
    public static class Autenticacion
    {
        public static byte[] GenerarHash(string contrasenia)
        {
            using (SHA256 shaHash = SHA256.Create())
            {
                byte[] bytes = shaHash.ComputeHash(
                    Encoding.UTF8.GetBytes(contrasenia));

                return bytes;
            }
        }

        public static string ObtenerHash(byte[] hash)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        public static JwtSecurityToken? leerToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.ReadToken(token) as JwtSecurityToken;
        }

        public static List<Claim> GenerarClaims(
            JwtSecurityToken? jwtToken,
            string accessToken)
        {
            if (jwtToken == null)
            {
                throw new ArgumentException(
                    "El token JWT no es válido.",
                    nameof(jwtToken));
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    jwtToken.Claims.First(
                        c => c.Type == ClaimTypes.Name).Value),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    jwtToken.Claims.First(
                        c => c.Type == ClaimTypes.NameIdentifier).Value),

                new Claim(
                    ClaimTypes.Email,
                    jwtToken.Claims.First(
                        c => c.Type == ClaimTypes.Email).Value),

                new Claim(
                    ClaimTypes.Role,
                    jwtToken.Claims.First(
                        c => c.Type == ClaimTypes.Role).Value),

                new Claim("Token", accessToken)
            };

            return claims;
        }
    }
}
