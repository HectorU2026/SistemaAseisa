using System.Data;
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA;

public class CuentaContableDA : ICuentaContableDA
{
    private readonly string _connectionString;

    public CuentaContableDA(IRepositorioDapper repositorio)
    {
        _connectionString = repositorio.ObtenerRepositorio().ConnectionString;
    }

    public async Task<IEnumerable<CuentaContableDetalleResponse>> Obtener(int idEmpresa, int? idCuentaContable = null, bool soloActivas = false)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QueryAsync<CuentaContableDetalleResponse>(
            "dbo.ConsultarCuentasContables", new { idEmpresa, idCuentaContable, soloActivas },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Agregar(int idEmpresa, int idUsuario, AgregarCuentaContableRequest datos)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>(
            "dbo.AgregarCuentaContable", new
            {
                idEmpresa,
                idUsuario,
                datos.IdCuentaPrincipal,
                datos.Codigo,
                datos.Nombre,
                datos.ConciliacionPagos,
                datos.IdEstado,
                datos.Impuestos,
                datos.Descripcion
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> AgregarRaiz(int idEmpresa, int idUsuario, AgregarCuentaRaizRequest datos)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>("dbo.AgregarCuentaContableRaiz",
            new
            {
                idEmpresa,
                idUsuario,
                datos.Codigo,
                datos.Nombre,
                datos.TipoCuenta,
                datos.Naturaleza,
                datos.ConciliacionPagos,
                datos.IdEstado,
                datos.Impuestos,
                datos.Descripcion
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Editar(int idEmpresa, int idUsuario, int idCuentaContable, EditarCuentaContableRequest datos)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>(
            "dbo.EditarCuentaContable", new
            {
                idEmpresa,
                idUsuario,
                idCuentaContable,
                datos.IdCuentaPrincipal,
                datos.Nombre,
                datos.ConciliacionPagos,
                datos.Impuestos,
                datos.Descripcion
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Inhabilitar(int idEmpresa, int idUsuario, int idCuentaContable)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>(
            "dbo.InhabilitarCuentaContable", new { idEmpresa, idUsuario, idCuentaContable },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> Habilitar(int idEmpresa, int idUsuario, int idCuentaContable)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<int>(
            "dbo.HabilitarCuentaContable", new { idEmpresa, idUsuario, idCuentaContable },
            commandType: CommandType.StoredProcedure);
    }
}
