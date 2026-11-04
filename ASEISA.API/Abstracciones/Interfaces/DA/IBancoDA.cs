using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IBancoDA
    {
        Task<string> RegistrarCuentaBancaria(CuentaBancariaRequest request);
        Task<IEnumerable<CuentaBancariaResponse>> ObtenerCuentasBancarias(int idEmpresa);
        Task<IEnumerable<TipoCuentaResponse>> ObtenerTiposCuenta();
        Task<IEnumerable<CuentaContableResponse>> ObtenerCuentasContables(int idEmpresa);
        Task<int> RegistrarPagoCliente(PagoClienteRequest pago);
        Task<MovimientoBancarioRespuesta> ObtenerMovimientosBancarios(MovimientoBancarioFiltro filtro);
    }
}
