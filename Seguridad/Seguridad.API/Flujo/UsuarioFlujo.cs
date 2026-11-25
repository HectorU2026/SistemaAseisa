using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Seguridad.Criptografia;

namespace Flujo
{
    public class UsuarioFlujo : IUsuarioFlujo
    {
        private IUsuarioDA _usuarioDA;
        private readonly IAuditoriaSeguridad _auditoria;

        public UsuarioFlujo(IUsuarioDA usuarioDA, IAuditoriaSeguridad auditoria)
        {
            _usuarioDA = usuarioDA;
            _auditoria = auditoria;
        }

        public async Task<RegistroResponse> CrearUsuario(RegistroRequest infoUusuario)
        {
            if (infoUusuario?.Usuario == null ||
                string.IsNullOrWhiteSpace(infoUusuario.Usuario.ContrasenaHash))
                throw new ArgumentException("La contraseña es obligatoria.");
            var usuario=infoUusuario.Usuario;
            var cliente=infoUusuario.Cliente;
            if(cliente==null || ASEISA.Usuarios.ReglasRegistroUsuario.Identificacion(usuario.TipoCliente,cliente.Identificacion,cliente.NombreRazonSocial,"NombreRazonSocial").Any())
                throw new ArgumentException("Datos de registro no validos.");
            usuario.NombreUsuario=ASEISA.Usuarios.ReglasRegistroUsuario.Generar(usuario.Nombre,usuario.PrimerApellido,cliente.Identificacion);
            infoUusuario.Usuario.ContrasenaHash = ContrasenasSeguras.Crear(infoUusuario.Usuario.ContrasenaHash);
            var resultado = await _usuarioDA.CrearUsuario(infoUusuario);
            if (resultado.IdUsuario is > 0)
                await _auditoria.RegistrarAsync(EventoAuditoriaSeguridad.RegistroPublicoExitoso, resultado.IdUsuario);
            return resultado;
        }

        public async Task<LoginAutenticado> ObtenerUsuario(LoginBase login)
        {
            return await _usuarioDA.ObtenerUsuario(login);
        }

    }
}
