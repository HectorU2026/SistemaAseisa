using ASEISA.Plataforma;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
namespace Flujo;
public sealed class PlataformaFlujo:IPlataformaFlujo
{
    private readonly IPlataformaDA da;
    public PlataformaFlujo(IPlataformaDA da){this.da=da;}
    public Task<VistaPlataforma> Consultar(int actor,int? empresa,CancellationToken ct)=>da.Consultar(actor,empresa,ct);
    public Task<int> EstadoEmpresa(int actor,int empresa,bool activo,CancellationToken ct)=>da.EstadoEmpresa(actor,empresa,activo,ct);
    public Task<int> Guardar(int actor,int? empresa,EmpresaPlataforma datos,CancellationToken ct)=>da.Guardar(actor,empresa,datos,ct);
    public Task<int> EditarAdministrador(int actor,int empresa,int usuario,EdicionAdministradorPlataforma datos,CancellationToken ct)=>da.EditarAdministrador(actor,empresa,usuario,datos,ct);
    public Task<int> EstadoAdministrador(int actor,int empresa,int usuario,bool activo,CancellationToken ct)=>da.EstadoAdministrador(actor,empresa,usuario,activo,ct);
}