using Microsoft.AspNetCore.Mvc;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IEnviarXmlFactura
    {
        Task<ActionResult> Enviar(int idUsuario, int idEmpresa, int idFactura);
    }
}
