using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Flujo;

public interface IDistribucionAnaliticaFlujo
{
    Task<IEnumerable<DistribucionAnaliticaResponse>> Obtener(int idEmpresa, int? idDistribucion = null);
    Task<int> Agregar(int idEmpresa, int idUsuario, DistribucionAnaliticaRequest datos);
    Task<int> Editar(int idEmpresa, int idUsuario, int idDistribucion, DistribucionAnaliticaRequest datos);
    Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idDistribucion);
}
