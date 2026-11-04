using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA
{
    public class ActivoFijoDA : IActivoFijoDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public ActivoFijoDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }
        public async Task<int> RegistrarActivoFijo(ActivoFijoRequest activo)
        {
            var sql = @"RegistrarActivoFijo";

            var resultado = await _sqlConnection.QueryAsync<int>(sql,
                new
                {
                    id_empresa = activo.IdEmpresa,
                    codigo = activo.Codigo,
                    nombre = activo.Nombre,
                    descripcion = activo.Descripcion,
                    fecha_adquisicion = activo.FechaAdquisicion,
                    costo_adquisicion = activo.CostoAdquisicion,
                    valor_residual = activo.ValorResidual,
                    vida_util_meses = activo.VidaUtilMeses,
                    metodo_depreciacion = activo.MetodoDepreciacion,
                    id_cuenta_activo = activo.IdCuentaActivo,
                    id_cuenta_depreciacion = activo.IdCuentaDepreciacion,
                    id_estado = activo.IdEstado,
                    id_usuario = activo.IdUsuario
                }, commandType: System.Data.CommandType.StoredProcedure);

            return resultado.FirstOrDefault();
        }
    }
}
