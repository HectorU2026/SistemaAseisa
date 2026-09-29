using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Microsoft.Data.SqlClient;
using Dapper;

namespace DA
{
    public class BitacoraDA : IBitacoraDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public BitacoraDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task RegistrarBitacora(Bitacora bitacora)
        {
            var sql = @"RegistrarBitacora";

            await _sqlConnection.ExecuteAsync(sql,
                new
                {
                    id_usuario = bitacora.IdUsuario,
                    id_empresa = bitacora.IdEmpresa,
                    tabla_afectada = bitacora.TablaAfectada,
                    registro_id = bitacora.RegistroId,
                    accion = bitacora.Accion,
                    valor_anterior = bitacora.ValorAnterior,
                    valor_nuevo = bitacora.ValorNuevo
                });
        }
    }
}
