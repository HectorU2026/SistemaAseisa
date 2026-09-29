using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IUsuarioDA
    {
        Task ActualizarUltimoAcceso(string correo);
        Task<Usuario> ObtenerInfoUsuario(Usuario usuario);
    }
}
