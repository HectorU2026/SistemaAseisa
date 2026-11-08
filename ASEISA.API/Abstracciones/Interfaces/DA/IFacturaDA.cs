using Abstracciones.Modelos;
using System.Collections.Generic;

namespace Abstracciones.Interfaces.DA
{
    public interface IFacturaDA
    {
        Task<IEnumerable<FacturaListado>> ObtenerFacturas(int idEmpresa, FacturaFiltro? filtro);
        Task<FacturaDetalle?> ObtenerFactura(int idEmpresa, int idFactura);
        Task<FacturaCambioEstadoResultado> Confirmar(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila);
        Task<FacturaCambioEstadoResultado> Restablecer(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila);
        Task<FacturaCambioEstadoResultado> Inactivar(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila, string motivoAnulacion);
        Task<FacturaCambioEstadoResultado> Reactivar(int idEmpresa, int idUsuario, int idFactura, byte[] versionFila);
        Task<FacturaPagoResultado> RegistrarPago(int idEmpresa, int idFactura, decimal monto, string metodoPago);
        Task<FacturaNotaCreditoResultado> RegistrarNotaCredito(int idEmpresa, int idUsuario, int idFactura, string motivo, decimal monto, string? descripcionAdicional);
    }
}
