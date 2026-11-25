using Abstracciones.Modelos;
namespace Abstracciones.DA
{
    public interface ICredencialesDA
    {
        Task<(int IdUsuario, DestinatarioContrasena? Destinatario)> Registrar(RegistroAdministrativoRequest solicitud,int empresa,int actor,string hash,byte[] hashToken,string tokenCifrado,CancellationToken ct);
        Task<DestinatarioContrasena?> Solicitar(byte[] hashToken,string tokenCifrado,string proposito,string? correo,int? usuario,int? empresa,int? actor,CancellationToken ct);
        Task<bool> Consumir(byte[] hashToken,string hashContrasena,CancellationToken ct);
    }
}