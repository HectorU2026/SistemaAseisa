using Abstracciones.Flujo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Globalization;
using System.Security.Claims;

namespace API.Seguridad
{
    public sealed class RevalidacionJwtEvents : JwtBearerEvents
    {
        private readonly IContextoUsuarioFlujo _contextoFlujo;
        private readonly ILogger<RevalidacionJwtEvents> _logger;
        private bool _consultaFallida;

        public RevalidacionJwtEvents(IContextoUsuarioFlujo contextoFlujo, ILogger<RevalidacionJwtEvents> logger)
        {
            _contextoFlujo = contextoFlujo;
            _logger = logger;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var usuarios = context.Principal?.FindAll(ClaimTypes.NameIdentifier).ToArray();
            var versiones = context.Principal?.FindAll("VersionCredencial").ToArray();
            var empresas = context.Principal?.FindAll("Empresa").ToArray();
            if (versiones?.Length != 1 || !long.TryParse(versiones[0].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var version) || version < 0
                || usuarios?.Length != 1 || empresas?.Length != 1
                || !int.TryParse(usuarios[0].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var idUsuario)
                || !int.TryParse(empresas[0].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var idEmpresa)
                || idUsuario <= 0 || idEmpresa <= 0)
            {
                context.Fail("Identidad no disponible.");
                return;
            }

            try
            {
                var vigente = await _contextoFlujo.Obtener(idUsuario, idEmpresa, context.HttpContext.RequestAborted);
                if (vigente == null || vigente.VersionCredencial != version)
                {
                    context.Fail("Identidad no disponible.");
                    return;
                }

                var claims = new List<Claim>
                {
                    new("VersionCredencial", vigente.VersionCredencial.Value.ToString(CultureInfo.InvariantCulture)),
                    new(ClaimTypes.NameIdentifier, vigente.IdUsuario.ToString(CultureInfo.InvariantCulture)),
                    new("Empresa", vigente.IdEmpresa.ToString(CultureInfo.InvariantCulture)),
                    new(ClaimTypes.Name, vigente.NombreUsuario),
                    new(ClaimTypes.Email, vigente.Correo)
                };
                foreach (var rol in vigente.Roles.Select(r => r.NombreRol).Distinct(StringComparer.OrdinalIgnoreCase))
                    claims.Add(new Claim(ClaimTypes.Role, rol));

                // No conservar identidades auxiliares, roles ni otros claims del JWT anterior.
                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(
                    claims, context.Scheme.Name, ClaimTypes.Name, ClaimTypes.Role));
            }
            catch (Exception)
            {
                _consultaFallida = true;
                // No incluir excepciones SQL, parametros, credenciales ni tokens en logs/respuestas.
                _logger.LogError(new EventId(1002, "RevalidacionIdentidadFallida"),
                    "No se pudo consultar el contexto de autorizacion.");
                context.Fail("Servicio de autorizacion no disponible.");
            }
        }

        public override async Task Challenge(JwtBearerChallengeContext context)
        {
            if (!_consultaFallida)
            {
                await base.Challenge(context);
                return;
            }
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Servicio temporalmente no disponible." });
        }
    }
}