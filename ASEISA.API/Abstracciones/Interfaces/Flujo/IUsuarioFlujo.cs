using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IUsuarioFlujo
    {
        Task ActualizarUltimoAcceso(string correo);
        Task<UsuarioResponse> ObtenerInfoUsuario(string correo);
        Task<int> AgregarUsuario(int idEmpresa, UsuarioRequest usuario);
    }
}
