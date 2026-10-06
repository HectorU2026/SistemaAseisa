using Abstracciones.DA;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Flujo
{
    public interface IUsuarioFlujo
    {
        Task<RegistroResponse> CrearUsuario(RegistroRequest infoUusuario);
        Task<LoginAutenticado> ObtenerUsuario(LoginBase login);

    }
}
