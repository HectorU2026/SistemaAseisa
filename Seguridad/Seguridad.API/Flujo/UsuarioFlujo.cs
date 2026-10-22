using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Seguridad.Criptografia;

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
            if (infoUusuario?.Usuario == null ||
                string.IsNullOrWhiteSpace(infoUusuario.Usuario.ContrasenaHash))
                throw new ArgumentException("La contraseña es obligatoria.");
            infoUusuario.Usuario.ContrasenaHash = ContrasenasSeguras.Crear(infoUusuario.Usuario.ContrasenaHash);
            return await _usuarioDA.CrearUsuario(infoUusuario);
        }

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            return await _usuarioDA.ObtenerUsuario(login);
        }

    }
}
