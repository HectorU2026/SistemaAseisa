using Abstracciones.Modelos;

namespace Abstracciones.Flujo
{
    public interface IContextoUsuarioFlujo
    {
        Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int? idEmpresa, CancellationToken cancellationToken);
    }
}
