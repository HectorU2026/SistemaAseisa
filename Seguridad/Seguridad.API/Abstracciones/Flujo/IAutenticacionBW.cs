using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Flujo
{
    public interface IAutenticacionFlujo
    {
        Task<Token> LoginAsync(LoginBase login);
        Task<IActionResult> RecuperarContrasena(UsuarioRecuperar correo);
        Task<IActionResult> CambiarContrasena(CambiarContrasena usuario);
    }
}
