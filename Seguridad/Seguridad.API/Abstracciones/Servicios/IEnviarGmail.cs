using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Servicios
{
    public interface IEnviarGmail
    {
        Task EnviarEmailUsuario(UsuarioSolicitado usuario, String enlace);
    }
}
