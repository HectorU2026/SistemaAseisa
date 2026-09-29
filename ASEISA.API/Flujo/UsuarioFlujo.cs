using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class UsuarioFlujo : IUsuarioFlujo   
    {
        private IUsuarioDA _usuarioDA;

        public UsuarioFlujo(IUsuarioDA usuarioDA)
        {
            _usuarioDA = usuarioDA;
        }

        public async Task ActualizarUltimoAcceso(string correo)
        {
            await _usuarioDA.ActualizarUltimoAcceso(correo);
        }   

        public async Task<Usuario> ObtenerInfoUsuario(Usuario usuario)
        {
            return await _usuarioDA.ObtenerInfoUsuario(usuario);
        }   
    }
}
