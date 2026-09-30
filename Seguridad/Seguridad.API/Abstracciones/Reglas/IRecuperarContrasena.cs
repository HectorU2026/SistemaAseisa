using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Reglas
{
    public interface IRecuperarContrasena
    {
        Task<ActionResult> GestionRecuperarContrasena(UsuarioRecuperar correo);

        string? ObtenerCorreoDelToken(string token);
    }
}
