using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IUsuarioDA
    {
        Task ActualizarUltimoAcceso(string correo);
        Task<UsuarioResponse> ObtenerInfoUsuario(string correo);
        Task<int> AgregarUsuario(int idEmpresa, UsuarioRequest usuario);
        Task<int> EditarUsuario(int idEmpresa, int idUsuario, UsuarioRequest usuario);
        Task<int> CambiarEstadoUsuario(int idEmpresa, int idUsuario, int idEstado);
    }
}
