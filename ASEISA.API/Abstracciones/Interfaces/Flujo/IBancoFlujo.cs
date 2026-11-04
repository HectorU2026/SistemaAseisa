using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.Flujo
{
    public interface IBancoFlujo
    {
        Task<string> RegistrarCuentaBancaria(CuentaBancariaRequest cuentaBancaria);
        Task<ActionResult> ObtenerCuentasBancarias(int idEmpresa);
        Task<ActionResult> ObtenerTiposCuenta();
        Task<ActionResult> ObtenerCuentasContables(int idEmpresa);
        Task<int> RegistrarPagoCliente(PagoClienteRequest pago);
        Task<MovimientoBancarioRespuesta> ObtenerMovimientosBancarios(MovimientoBancarioFiltro filtro);
    }
}
