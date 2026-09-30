using Abstracciones.DA;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Abstracciones.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Servicios;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reglas
{
    public class RecuperarContrasena : IRecuperarContrasena
    {
        public IConfiguration _configuration;
        public IUsuarioDA _usuarioDA;
        private UsuarioRecuperar _usuario;
        private IEnviarGmail _enviarGmail;

        public RecuperarContrasena(IConfiguration configuration, IUsuarioDA usuarioDA, IEnviarGmail enviarGmail)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
            _enviarGmail = enviarGmail;
        }

        public async Task<ActionResult> GestionRecuperarContrasena(UsuarioRecuperar correo)
        {
            _usuario = await _usuarioDA.ValidarCorreo(correo.correo);
            if (_usuario == null)
            {
                return new BadRequestObjectResult("Correo no encontrado");
            }

            TokenConfiguracion tokenConfiguracion = _configuration.GetSection("Token").Get<TokenConfiguracion>();

            var jwt = await GenerarTokenRecuperarContrasena(correo.correo, tokenConfiguracion);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwt);

            var urlBase = _configuration["App:BaseUrl"];
            var enlace = $"{urlBase}?token={Uri.EscapeDataString(token)}";

            UsuarioSolicitado usuario = new UsuarioSolicitado
            {
                Nombre = _usuario.Nombre,
                correo = correo.correo,
                token = token
            };

            await _enviarGmail.EnviarEmailUsuario(usuario, enlace);

            return new OkObjectResult(_usuario);
        }

        private async Task<JwtSecurityToken> GenerarTokenRecuperarContrasena(string correoelectronico, TokenConfiguracion tokenConfiguracion)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfiguracion.key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new System.Security.Claims.Claim("correo", correoelectronico) };
            var token = new JwtSecurityToken(tokenConfiguracion.Issuer, tokenConfiguracion.Audience, claims: claims, expires: DateTime.Now.AddMinutes(tokenConfiguracion.Expires), signingCredentials: credentials);
            return token;
        }

        public string? ObtenerCorreoDelToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "correo")?.Value;
        }
    }
}
