using ASEISA.Plataforma;
using Abstracciones.Modelos;
namespace Abstracciones.Flujo;
public interface IPlataformaCredencialesFlujo
{
    Task<ResultadoInvitacion> Registrar(int actor,int empresa,PersonaPlataforma persona,CancellationToken ct);
    Task<ResultadoInvitacion> Restablecer(int actor,int empresa,int usuario,CancellationToken ct);
    Task<int> Inicializar(PersonaPlataforma persona,string destino,CancellationToken ct);
}