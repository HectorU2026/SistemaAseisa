using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<UsuarioResponse> ObtenerInfoUsuario(string correo)
        {
            return await _usuarioDA.ObtenerInfoUsuario(correo);
        }   

        public async Task<int> AgregarUsuario(int idEmpresa, UsuarioRequest usuario)
        {
            return await _usuarioDA.AgregarUsuario(idEmpresa, usuario);
        }
        
        public async Task<int> EditarUsuario(int idEmpresa, int idUsuario, UsuarioRequest usuario)
        {
            return await _usuarioDA.EditarUsuario(idEmpresa, idUsuario, usuario);
        }
        
        public async Task<int> CambiarEstadoUsuario(int idEmpresa, int idUsuario, int idEstado)
        {
            return await _usuarioDA.CambiarEstadoUsuario(idEmpresa, idUsuario, idEstado);
        }

        public async Task<int> RestablecerContrasenaUsuario(int idEmpresa, int idUsuario, string contrasenaHash)
        {
            return await _usuarioDA.RestablecerContrasenaUsuario(idEmpresa, idUsuario, contrasenaHash);
        }

        public async Task<int> AsignarRolUsuario(int idEmpresa, int idUsuario, int idRol)
        {
            return await _usuarioDA.AsignarRolUsuario(idEmpresa, idUsuario, idRol);
        }

    }
}
