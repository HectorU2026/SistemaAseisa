using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA
{
    public interface IMetodoPagoDA
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
