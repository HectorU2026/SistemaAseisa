using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.DA
{ 
    public interface IUsuarioDA
    {
        Task<RegistroResponse> CrearUsuario(RegistroRequest infoUsuario);
        Task<IEnumerable<LoginAutenticado>> ObtenerCandidatoAutenticacion(LoginBase login);
        Task<LoginAutenticado> ObtenerUsuario(LoginBase login);
        Task<IEnumerable<LoginAutenticado>> ObtenerUsuarioConRoles(LoginBase login);
        Task<UsuarioRecuperar> ValidarCorreoActivo(string correo);
        Task<bool> MigrarHashAutenticacion(LoginAutenticado usuario, string hashNuevo);

    }
}
