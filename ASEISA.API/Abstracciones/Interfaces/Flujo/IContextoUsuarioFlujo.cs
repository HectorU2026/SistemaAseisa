using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IContextoUsuarioFlujo
    {
        Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int idEmpresa, CancellationToken cancellationToken);
    }
}
