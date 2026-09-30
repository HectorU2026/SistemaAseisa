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
        Task<int> CrearUsuario(RegistroRequest infoUsuario);
        Task<LoginAutenticado> ObtenerUsuario(LoginBase login);
        Task<UsuarioRecuperar> ValidarCorreo(string correo);
        Task<ActionResult> CambiarContrasena(CambiarContrasena usuario);

    }
}
