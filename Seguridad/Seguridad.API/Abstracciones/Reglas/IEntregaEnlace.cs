using Abstracciones.Modelos;
namespace Abstracciones.Reglas
{
    public interface IEntregaEnlace
    {
        void ValidarConfiguracion();
        Task<bool> Enviar(DestinatarioContrasena destinatario,string token);
    }
}