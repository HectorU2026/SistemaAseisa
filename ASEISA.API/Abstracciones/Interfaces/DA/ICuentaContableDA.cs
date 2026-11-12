using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA;

public interface ICuentaContableDA
{
    Task<IEnumerable<CuentaContableDetalleResponse>> Obtener(int idEmpresa, int? idCuentaContable = null, bool soloActivas = false);
    Task<int> AgregarRaiz(int idEmpresa, int idUsuario, AgregarCuentaRaizRequest datos);
    Task<int> Agregar(int idEmpresa, int idUsuario, AgregarCuentaContableRequest datos);
    Task<int> Editar(int idEmpresa, int idUsuario, int idCuentaContable, EditarCuentaContableRequest datos);
    Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idCuentaContable);
    Task<int> Habilitar(int idEmpresa, int idUsuario, int idCuentaContable);
}
