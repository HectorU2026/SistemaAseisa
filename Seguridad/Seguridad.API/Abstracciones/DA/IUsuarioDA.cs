using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.DA
{ 
    public interface IUsuarioDA
    {
        Task<RegistroResponse> CrearUsuario(RegistroRequest infoUsuario);
        Task<LoginAutenticado> ObtenerUsuario(LoginBase login);
        Task<UsuarioRecuperar> ValidarCorreoActivo(string correo);
        Task<ActionResult> CambiarContrasena(CambiarContrasena usuario);

    }
}
