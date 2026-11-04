using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA
{
    public interface ITerminoPagoDA
    {
        Task<IEnumerable<TerminoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario);

        Task<TerminoPagoCreadoResponse> Agregar(
            int idEmpresa, int idUsuario,
            TerminoPagoRequest termino);

        Task Editar(
            int idEmpresa, int idUsuario, int idTerminoPago,
            TerminoPagoRequest termino);

        Task CambiarEstado(
            int idEmpresa, int idUsuario, int idTerminoPago,
            bool activo);
    }
}