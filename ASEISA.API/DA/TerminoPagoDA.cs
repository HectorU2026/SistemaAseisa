using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DA
{
    public class TerminoPagoDA : ITerminoPagoDA
    {
        private readonly string _connectionString;

        public TerminoPagoDA(IRepositorioDapper repositorioDapper)
        {
            _connectionString = repositorioDapper.ObtenerRepositorio().ConnectionString;
        }

        public async Task<IEnumerable<TerminoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario)
        {
            using var conexion = new SqlConnection(_connectionString);

            var contexto = await conexion.QueryAsync<TerminoPagoResponse>(
                "dbo.ObtenerTerminosPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario
                });
            return contexto;
        }

        public async Task<TerminoPagoCreadoResponse> Agregar(
            int idEmpresa,
            int idUsuario,
            TerminoPagoRequest termino)
        {
            using var conexion = new SqlConnection(_connectionString);

            var resultado = await conexion
                .QuerySingleAsync<TerminoPagoCreadoResponse>(
                    "dbo.AgregarTerminoPago",
                    new
                    {
                        id_empresa = idEmpresa,
                        id_usuario = idUsuario,
                        nombre = termino.Nombre,
                        dias_credito = termino.DiasCredito
                    },
                    commandType: CommandType.StoredProcedure);

            return resultado;
        }

        public async Task Editar(
            int idEmpresa,
            int idUsuario,
            int idTerminoPago,
            TerminoPagoRequest termino)
        {
            using var conexion = new SqlConnection(_connectionString);

            await conexion.ExecuteAsync(
                "dbo.EditarTerminoPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_termino_pago = idTerminoPago,
                    nombre = termino.Nombre,
                    dias_credito = termino.DiasCredito
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task CambiarEstado(
            int idEmpresa,
            int idUsuario,
            int idTerminoPago,
            bool activo)
        {
            using var conexion = new SqlConnection(_connectionString);

            await conexion.ExecuteAsync(
                "dbo.CambiarEstadoTerminoPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_termino_pago = idTerminoPago,
                    activo
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
