using Abstracciones.Reglas;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abstracciones.DA;

namespace Flujo
{
    public class AutenticacionFlujo : IAutenticacionFlujo
    {

        private IAutenticacionBC _autenticacionBC;
        private IRecuperarContrasena _recuperarContrasena;
        private IUsuarioDA _usuarioDA; 

        public AutenticacionFlujo(IAutenticacionBC autenticacionBC, IRecuperarContrasena recuperarContrasena, IUsuarioDA usuario)
        {
            _recuperarContrasena = recuperarContrasena;
            _autenticacionBC = autenticacionBC;
            _usuarioDA = usuario;
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
            var correo = _recuperarContrasena.ObtenerCorreoDelToken(usuario.Token);

            if (string.IsNullOrEmpty(correo))
                return new BadRequestObjectResult("Token inválido"); 
            usuario.correo = correo;
            return await _usuarioDA.CambiarContrasena(usuario);
        }
    }
}
