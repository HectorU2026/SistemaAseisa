using Abstracciones.DA;
using Abstracciones.Modelos;
using Dapper;
using System.Data.SqlClient;

namespace DA
{
    public class AuditoriaSeguridadDA : IAuditoriaSeguridadDA
    {
        private readonly IRepositorioDapper _repositorio;

        public AuditoriaSeguridadDA(IRepositorioDapper repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task RegistrarAsync(EventoAuditoriaSeguridad evento, int? idUsuario, string? correo)
        {
            var accion = evento switch
            {
                EventoAuditoriaSeguridad.LoginExitoso => "Login",
                EventoAuditoriaSeguridad.RegistroPublicoExitoso => "RegistrarUsuarioCliente",
                EventoAuditoriaSeguridad.CambioContrasenaExitoso => "CambiarContrasena",
                _ => throw new ArgumentOutOfRangeException(nameof(evento))
            };
            if (evento == EventoAuditoriaSeguridad.CambioContrasenaExitoso)
            {
                if (idUsuario != null || string.IsNullOrWhiteSpace(correo))
                    throw new ArgumentException("El cambio requiere el correo del token validado.");
            }
            else if (idUsuario is null or <= 0 || correo != null)
                throw new ArgumentException("El evento requiere el usuario confirmado por el servidor.");

            // Empresa y actor proceden de USUARIO; no se reciben desde Web.
            // Una conexion independiente evita compartir la conexion del flujo principal.
            using var conexion = new SqlConnection(_repositorio.ObtenerRepositorioDapper().ConnectionString);
            const string sql = @"
                SET NOCOUNT ON;
                INSERT INTO dbo.BITACORA
                    (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
                SELECT U.id_usuario, U.id_empresa, @tabla, CONVERT(VARCHAR(100), U.id_usuario),
                       @accion, NULL, NULL
                FROM dbo.USUARIO U
                WHERE (@idUsuario IS NOT NULL AND U.id_usuario = @idUsuario)
                   OR (@idUsuario IS NULL AND U.correo = @correo);
                SELECT @@ROWCOUNT;";
            var filas = await conexion.QuerySingleAsync<int>(sql, new
            {
                idUsuario,
                correo,
                accion,
                tabla = evento == EventoAuditoriaSeguridad.RegistroPublicoExitoso ? "Usuario, Cliente" : "Usuario"
            });
            if (filas != 1)
                throw new InvalidOperationException("No se encontro una identidad unica para auditar.");
        }
    }
}
