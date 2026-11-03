using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DA
{
    public class BancoDA : IBancoDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public BancoDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<string> RegistrarCuentaBancaria(CuentaBancariaRequest request)
        {
            var sql = "RegistrarCuentaBancaria";

            var resultado = await _sqlConnection.QueryFirstOrDefaultAsync<string>(
                sql,
                new
                {
                    id_empresa = request.IdEmpresa,
                    numero_cuenta = request.NumeroCuenta,
                    id_tipo_cuenta = request.IdTipoCuenta,
                    moneda = request.Moneda,
                    saldo_actual = request.SaldoActual,
                    id_cuenta_contable = request.IdCuentaContable,
                    id_estado = request.IdEstado
                });

            return resultado;
        }

        public async Task<IEnumerable<CuentaBancariaResponse>> ObtenerCuentasBancarias(int idEmpresa)
        {
            var sql = @"ObtenerCuentasBancarias";

            var resultado = await _sqlConnection.QueryAsync<CuentaBancariaResponse>(
                sql,
                new 
                { 
                    id_empresa = idEmpresa 
                });

            return resultado;
        }




        #region "Catalogo"
        public async Task<IEnumerable<TipoCuentaResponse>> ObtenerTiposCuenta()
        {
            var sql = @"ObtenerTiposCuenta";
            return await _sqlConnection.QueryAsync<TipoCuentaResponse>(sql);
        }

        public async Task<IEnumerable<CuentaContableResponse>> ObtenerCuentasContables(int idEmpresa)
        {
            var sql = @"ObtenerCuentasContables";
            return await _sqlConnection.QueryAsync<CuentaContableResponse>(
                sql,
                new 
                { 
                    id_empresa = idEmpresa 
                });
        }
        #endregion


    }
}
