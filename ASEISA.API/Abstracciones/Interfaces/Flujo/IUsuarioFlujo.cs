using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IUsuarioFlujo
    {
        Task ActualizarUltimoAcceso(string correo);
        Task<Usuario> ObtenerInfoUsuario(string correo);
    }
}
