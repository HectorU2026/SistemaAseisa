using System.ComponentModel.DataAnnotations;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo;

public class CuentaContableFlujo(ICuentaContableDA da) : ICuentaContableFlujo
{
    public Task<IEnumerable<CuentaContableDetalleResponse>> Obtener(int idEmpresa, int? idCuentaContable = null, bool soloActivas = false)
    {
        return da.Obtener(idEmpresa, idCuentaContable, soloActivas);
    }

    public Task<int> Agregar(int idEmpresa, int idUsuario, AgregarCuentaContableRequest datos)
    {
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        return da.Agregar(idEmpresa, idUsuario, datos);
    }

    public Task<int> AgregarRaiz(int idEmpresa, int idUsuario, AgregarCuentaRaizRequest datos)
    {
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        return da.AgregarRaiz(idEmpresa, idUsuario, datos);
    }

    public Task<int> Editar(int idEmpresa, int idUsuario, int idCuentaContable, EditarCuentaContableRequest datos)
    {
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        return da.Editar(idEmpresa, idUsuario, idCuentaContable, datos);
    }

    public Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idCuentaContable)
    {
        return da.Inhabilitar(idEmpresa, idUsuario, idCuentaContable);
    }

    public Task<int> Habilitar(int idEmpresa, int idUsuario, int idCuentaContable)
    {
        return da.Habilitar(idEmpresa, idUsuario, idCuentaContable);
    }
}
