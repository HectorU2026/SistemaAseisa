using System.Data;
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA;

public class DistribucionAnaliticaDA : IDistribucionAnaliticaDA
{
    private readonly string _connectionString;

    public DistribucionAnaliticaDA(IRepositorioDapper repositorio)
    {
        var conexion = repositorio.ObtenerRepositorio();
        _connectionString = conexion.ConnectionString;
    }

    public async Task<IEnumerable<DistribucionAnaliticaResponse>> Obtener(int idEmpresa, int? idDistribucion = null)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QueryAsync<DistribucionAnaliticaResponse>("dbo.ObtenerDistribucionesAnaliticas",
            new { idEmpresa, idDistribucion }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Agregar(int idEmpresa, int idUsuario, DistribucionAnaliticaRequest datos)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>("dbo.AgregarDistribucionAnalitica",
            new { idEmpresa, idUsuario, datos.IdProveedor, datos.IdProducto, datos.Importe, datos.Descripcion, datos.Referencia, datos.Unidad }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Editar(int idEmpresa, int idUsuario, int idDistribucion, DistribucionAnaliticaRequest datos)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>("dbo.EditarDistribucionAnalitica",
            new { idEmpresa, idUsuario, idDistribucion, datos.IdProveedor, datos.IdProducto, datos.Importe, datos.Descripcion, datos.Referencia, datos.Unidad }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idDistribucion)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>("dbo.InhabilitarDistribucionAnalitica",
            new { idEmpresa, idUsuario, idDistribucion }, commandType: CommandType.StoredProcedure);
    }
}
