using ASEISA.Plataforma;
namespace Abstracciones.Interfaces.DA;
public interface IPlataformaDA
{
    Task<VistaPlataforma> Consultar(int actor,int? empresa,CancellationToken ct);
    Task<int> EstadoEmpresa(int actor,int empresa,bool activo,CancellationToken ct);
    Task<int> Guardar(int actor,int? empresa,EmpresaPlataforma datos,CancellationToken ct);
    Task<int> EditarAdministrador(int actor,int empresa,int usuario,EdicionAdministradorPlataforma datos,CancellationToken ct);
    Task<int> EstadoAdministrador(int actor,int empresa,int usuario,bool activo,CancellationToken ct);
}