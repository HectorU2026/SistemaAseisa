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

        public async Task<ActionResult> CrearUsuario(RegistroRequest infoUusuario)
        {
            if (await _usuarioDA.ValidarCorreo(infoUusuario.Usuario.Correo) != null)
                return new BadRequestObjectResult("El correo ya está registrado");
            var id = await _usuarioDA.CrearUsuario(infoUusuario);
            return new OkObjectResult(new { IdUsuario = id });        
        }

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            return await _usuarioDA.ObtenerUsuario(login);
        }

    }
}
