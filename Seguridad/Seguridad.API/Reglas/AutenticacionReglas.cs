
using Abstracciones.Reglas;
using Abstracciones.Flujo;
using Abstracciones.DA;
using Abstracciones.Modelos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Seguridad.Criptografia;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BC
{
    public class AutenticacionReglas : IAutenticacionBC
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioDA _usuarioDA;
        private readonly IAuditoriaSeguridad _auditoria;

        public AutenticacionReglas(
            IConfiguration configuration,
            IUsuarioDA usuarioDA,
            IAuditoriaSeguridad auditoria)
        {
            _configuration = configuration;
            _usuarioDA = usuarioDA;
            _auditoria = auditoria;
        }

        public async Task<Token> LoginAync(LoginBase login)
        {
            var resultado = new Token
            {
                AccessToken = string.Empty,
                ValidacionExitosa = false
            };

            if (login == null ||
                string.IsNullOrWhiteSpace(login.Correo) ||
                string.IsNullOrWhiteSpace(login.ContrasenaHash))
            {
                return resultado;
            }

            var filasUsuario = (await _usuarioDA.ObtenerCandidatoAutenticacion(login))
                .ToList();

            var usuario = filasUsuario.FirstOrDefault();

            if (usuario == null)
                return resultado;

            if(!usuario.VersionCredencial.HasValue || usuario.VersionCredencial.Value < 0)
                throw new InvalidOperationException("Contrato interno de autenticacion invalido.");
            bool credencialesValidas = ContrasenasSeguras.Verificar(
                login.ContrasenaHash,
                usuario.ContrasenaHash,
                out bool requiereMigracion);

            if (!credencialesValidas)
                return resultado;

            // Solo revelar el estado después de verificar el secreto. Sin JWT para cuentas no elegibles.
            resultado.CodigoEstado=usuario.EstadoAprobacion switch {
                "Pendiente"=>"PENDIENTE", "Rechazado"=>"RECHAZADO",
                "Aprobado" when !usuario.UsuarioActivo=>"INACTIVO",
                "Aprobado" when !usuario.EmpresaActiva=>"NO_DISPONIBLE",
                "Aprobado" when !filasUsuario.Any(f=>!string.IsNullOrWhiteSpace(f.rol))=>"NO_DISPONIBLE",
                "Aprobado"=>null, _=>"NO_DISPONIBLE" };
            if(resultado.CodigoEstado!=null)return resultado;

            var rolesVigentes=filasUsuario.Select(f=>f.rol).Where(r=>!string.IsNullOrWhiteSpace(r)).Distinct().ToArray();
            var plataforma=usuario.IdEmpresa==null;
            if(plataforma ? rolesVigentes.Length!=1 || rolesVigentes[0]!="Superadministrador" : rolesVigentes.Contains("Superadministrador"))return resultado;
            if (requiereMigracion)
            {
                // Una migracion del formato del mismo secreto no revoca sesiones,
                // pero tampoco puede sobrescribir un restablecimiento concurrente.
                if (!await _usuarioDA.MigrarHashAutenticacion(usuario, ContrasenasSeguras.Crear(login.ContrasenaHash)))
                    return resultado;
            }

            var cfg = _configuration
                .GetSection("Token")
                .Get<TokenConfiguracion>()
                ?? throw new InvalidOperationException(
                    "Falta la configuración Token.");

            var llave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(cfg.key));

            var credenciales = new SigningCredentials(
                llave,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("VersionCredencial", usuario.VersionCredencial.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo),
                new Claim(plataforma?"Ambito":"Empresa",plataforma?"Plataforma":usuario.IdEmpresa!,ClaimValueTypes.String)
            };

            var rolesActivos = filasUsuario
                .Select(fila => fila.rol)
                .Where(rol => !string.IsNullOrWhiteSpace(rol))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var rol in rolesActivos)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var jwt = new JwtSecurityToken(
                cfg.Issuer,
                cfg.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(cfg.Expires),
                signingCredentials: credenciales);

            resultado.AccessToken =
                new JwtSecurityTokenHandler().WriteToken(jwt);

            resultado.ValidacionExitosa = true;
            await _auditoria.RegistrarAsync(EventoAuditoriaSeguridad.LoginExitoso, usuario.IdUsuario);

            return resultado;
        }
    }
}
