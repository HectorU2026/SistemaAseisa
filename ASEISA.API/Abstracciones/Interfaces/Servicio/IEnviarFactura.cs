using Microsoft.AspNetCore.Mvc;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IEnviarFactura
    {
        Task<ActionResult> Enviar(int idUsuario, int idEmpresa, int idFactura);
    }
}
