using Abstracciones.Modelos;

namespace Abstracciones.DA
{
    public interface IContextoUsuarioDA
    {
        Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int? idEmpresa, CancellationToken cancellationToken);
    }
}
