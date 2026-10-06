using Abstracciones.Reglas;
using Abstracciones.DA;
using Abstracciones.Modelos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BC
{
    public class AutenticacionReglas : IAutenticacionBC
    {
        public IConfiguration _configuration;
        public IUsuarioDA _usuarioDA;
        private LoginAutenticado _usuario;

        public AutenticacionReglas(IConfiguration configuration, IUsuarioDA usuarioDA)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
        }

        public async Task<Token> LoginAync(LoginBase login) 
        {
            Token respuestaToken= new Token() { AccessToken=string.Empty,ValidacionExitosa=false };
            _usuario = await _usuarioDA.ObtenerUsuario(login); 
            if (_usuario == null)
                return respuestaToken;
            var resultadoVerificacionCredenciales = await VerificarHashContraseniaAsync(login);
            if (!resultadoVerificacionCredenciales)
                return respuestaToken;
            TokenConfiguracion tokenConfiguracion=_configuration.GetSection("Token").Get<TokenConfiguracion>();
            JwtSecurityToken token = await GenerarTokenJWT(login,tokenConfiguracion);
            respuestaToken.AccessToken=new JwtSecurityTokenHandler().WriteToken(token);
            respuestaToken.ValidacionExitosa = true;
            return respuestaToken;
        }

        private async Task<bool> VerificarHashContraseniaAsync(LoginBase login)
        {
            return (login != null && login.ContrasenaHash == _usuario.ContrasenaHash);

        }

        private async Task<JwtSecurityToken> GenerarTokenJWT(LoginBase login, TokenConfiguracion tokenConfiguracion)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfiguracion.key));
            List<Claim> claims = await GenerarClaims();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(tokenConfiguracion.Issuer, tokenConfiguracion.Audience, claims, expires: DateTime.Now.AddMinutes(tokenConfiguracion.Expires), signingCredentials: credentials);
            return token;
        }

        private async Task<List<Claim>> GenerarClaims()
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _usuario.id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, _usuario.NombreUsuario));
            claims.Add(new Claim(ClaimTypes.Role, _usuario.rol));
            claims.Add(new Claim(ClaimTypes.Email, _usuario.Correo));
            return claims;
        }

    }
}
