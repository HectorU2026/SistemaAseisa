using System.ComponentModel.DataAnnotations;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo;

public class DistribucionAnaliticaFlujo(IDistribucionAnaliticaDA da) : IDistribucionAnaliticaFlujo
{

    public Task<IEnumerable<DistribucionAnaliticaResponse>> Obtener(int idEmpresa, int? idDistribucion = null)
    {
        return da.Obtener(idEmpresa, idDistribucion);
    }

    public Task<int> Agregar(int idEmpresa, int idUsuario, DistribucionAnaliticaRequest datos)
    {
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        return da.Agregar(idEmpresa, idUsuario, datos);
    }

    public Task<int> Editar(int idEmpresa, int idUsuario, int idDistribucion, DistribucionAnaliticaRequest datos)
    {
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        return da.Editar(idEmpresa, idUsuario, idDistribucion, datos);
    }

    public Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idDistribucion)
    {
        return da.Inhabilitar(idEmpresa, idUsuario, idDistribucion);
    }
}
