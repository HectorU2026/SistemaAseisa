using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flujo
{
    public class UsuarioFlujo : IUsuarioFlujo
    {
        private IUsuarioDA _usuarioDA;

        public UsuarioFlujo(IUsuarioDA usuarioDA)
        {
            _usuarioDA = usuarioDA;
        }

        public async Task<RegistroResponse> CrearUsuario(RegistroRequest infoUusuario)
        {
            return await _usuarioDA.CrearUsuario(infoUusuario);
        }

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            return await _usuarioDA.ObtenerUsuario(login);
        }

    }
}
