using Abstracciones.Modelos;
namespace Abstracciones.DA
{
    public interface ICorreoEnlaceDA
    {
        Task<CorreoEnlacePendiente?> Tomar(CancellationToken ct);
        Task Finalizar(long id,Guid reserva,bool enviado,CancellationToken ct);
    }
}