using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IMetodoPagoFlujo
    {
        Task<IEnumerable<MetodoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario);

        Task<MetodoPagoCreadoResponse> Agregar(
            int idEmpresa, int idUsuario,
            MetodoPagoRequest metodo);

        Task Editar(
            int idEmpresa, int idUsuario, int idMetodoPago,
            MetodoPagoRequest metodo);

        Task CambiarEstado(
            int idEmpresa, int idUsuario, int idMetodoPago,
            bool activo);
    }
}
