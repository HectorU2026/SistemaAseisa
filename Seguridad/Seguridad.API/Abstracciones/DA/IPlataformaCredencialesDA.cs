using ASEISA.Plataforma;
namespace Abstracciones.DA;
public interface IPlataformaCredencialesDA
{
    Task<(int Rol,int Estado)> Validar(int actor,int empresa,int? usuario,CancellationToken ct);
    Task<int> Inicializar(PersonaPlataforma persona,string usuario,string hash,byte[] resumen,string cifrado,string destino,CancellationToken ct);
}