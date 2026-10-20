using Abstracciones.DA;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Servicios;
using Servicios.Correo;
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
        private readonly IEnviarCorreo _enviarCorreo;
        private readonly IConstructorCorreo _constructorCorreos;

        public RecuperarContrasena(IConfiguration configuration, IUsuarioDA usuarioDA, IEnviarCorreo enviarGmail, IConstructorCorreo constructorCorreos)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
            _enviarCorreo = enviarGmail;
            _constructorCorreos = constructorCorreos;
        }

        public async Task<ActionResult> GestionRecuperarContrasena(UsuarioRecuperar correo)
        {
            _usuario = await _usuarioDA.ValidarCorreoActivo(correo.correo);
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

            var contenido = await _constructorCorreos.RecuperarContrasena(_usuario.Nombre, enlace);
            await _enviarCorreo.EnviarEmailUsuario(_usuario.correo, contenido.Asunto, contenido.Html, contenido.TextoPlano);

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
