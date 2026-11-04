
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DA
{
    public class MetodoPagoDA : IMetodoPagoDA
    {
        private readonly string _connectionString;

        public MetodoPagoDA(IRepositorioDapper repositorioDapper)
        {
            _connectionString = repositorioDapper.ObtenerRepositorio().ConnectionString;
        }

        public async Task<IEnumerable<MetodoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario)
        {
            using var conexion = new SqlConnection(_connectionString);

            return await conexion.QueryAsync<MetodoPagoResponse>(
                "dbo.ObtenerMetodosPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<MetodoPagoCreadoResponse> Agregar(
            int idEmpresa,
            int idUsuario,
            MetodoPagoRequest metodo)
        {
            using var conexion = new SqlConnection(_connectionString);

            var resultado = await conexion
                .QuerySingleAsync<MetodoPagoCreadoResponse>(
                    "dbo.AgregarMetodoPago",
                    new
                    {
                        id_empresa = idEmpresa,
                        id_usuario = idUsuario,
                        nombre = metodo.Nombre
                    },
                    commandType: CommandType.StoredProcedure);

            return resultado;
        }

        public async Task Editar(
            int idEmpresa,
            int idUsuario,
            int idMetodoPago,
            MetodoPagoRequest metodo)
        {
            using var conexion = new SqlConnection(_connectionString);

            await conexion.ExecuteAsync(
                "dbo.EditarMetodoPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_metodo_pago = idMetodoPago,
                    nombre = metodo.Nombre
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task CambiarEstado(
            int idEmpresa,
            int idUsuario,
            int idMetodoPago,
            bool activo)
        {
            using var conexion = new SqlConnection(_connectionString);

            await conexion.ExecuteAsync(
                "dbo.CambiarEstadoMetodoPago",
                new
                {
                    id_empresa = idEmpresa,
                    id_usuario = idUsuario,
                    id_metodo_pago = idMetodoPago,
                    activo
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
