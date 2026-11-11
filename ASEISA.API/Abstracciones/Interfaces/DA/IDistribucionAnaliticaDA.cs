using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA;

public interface IDistribucionAnaliticaDA
{
    Task<IEnumerable<DistribucionAnaliticaResponse>> Obtener(int idEmpresa, int? idDistribucion = null);
    Task<int> Agregar(int idEmpresa, int idUsuario, DistribucionAnaliticaRequest datos);
    Task<int> Editar(int idEmpresa, int idUsuario, int idDistribucion, DistribucionAnaliticaRequest datos);
    Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idDistribucion);
}
