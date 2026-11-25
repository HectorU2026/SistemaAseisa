using Abstracciones.Modelos;
using Abstracciones.DA;
using Abstracciones.Flujo;

namespace Flujo
{
    public sealed class ContextoUsuarioFlujo : IContextoUsuarioFlujo
    {
        private readonly IContextoUsuarioDA _contextoDA;
        // Servicio scoped: reutilizar solo dentro de esta solicitud, nunca entre solicitudes.
        private readonly Dictionary<(int Usuario, int? Empresa), Task<ContextoUsuarioAutorizado?>> _consultas = new();

        public ContextoUsuarioFlujo(IContextoUsuarioDA contextoDA)
        {
            _contextoDA = contextoDA;
        }

        public Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int? idEmpresa, CancellationToken cancellationToken)
        {
            if (idUsuario <= 0 || idEmpresa <= 0)
                return Task.FromResult<ContextoUsuarioAutorizado?>(null);
            lock (_consultas)
            {
                var clave = (idUsuario, idEmpresa);
                if (!_consultas.TryGetValue(clave, out var consulta))
                {
                    consulta = Consultar(idUsuario, idEmpresa, cancellationToken);
                    _consultas.Add(clave, consulta);
                }
                return consulta;
            }
        }

        private async Task<ContextoUsuarioAutorizado?> Consultar(int idUsuario, int? idEmpresa, CancellationToken cancellationToken)
        {
            var contexto = await _contextoDA.Obtener(idUsuario, idEmpresa, cancellationToken);
            if (contexto == null) return null;
            if (contexto.IdUsuario != idUsuario || contexto.IdEmpresa != idEmpresa)
                return null;
            if (!contexto.VersionCredencial.HasValue || contexto.VersionCredencial.Value < 0 || string.IsNullOrWhiteSpace(contexto.NombreUsuario) || string.IsNullOrWhiteSpace(contexto.Correo)
                || contexto.Roles == null || contexto.Roles.Any(r => r.IdRol <= 0 || string.IsNullOrWhiteSpace(r.NombreRol)))
                throw new InvalidOperationException("Contexto de autorizacion invalido.");
            var plataforma=contexto.IdEmpresa==null;
            var super=contexto.Roles.Any(r=>r.NombreRol=="Superadministrador");
            if((plataforma && (!super || contexto.Roles.Any(r=>r.NombreRol!="Superadministrador"))) || (!plataforma && super))return null;
            contexto.Roles = contexto.Roles.DistinctBy(r => r.IdRol).ToList();
            return contexto;
        }
    }
}