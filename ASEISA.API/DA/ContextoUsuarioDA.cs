using Abstracciones.Modelos;
using Abstracciones.Interfaces.DA;
using DA.Repositorio;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace DA
{
    public sealed class ContextoUsuarioDA : IContextoUsuarioDA
    {
        private readonly IRepositorioDapper _repositorio;

        public ContextoUsuarioDA(IRepositorioDapper repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int? idEmpresa, CancellationToken cancellationToken)
        {
            using var conexion = new SqlConnection(_repositorio.ObtenerRepositorio().ConnectionString);
            var fila = await conexion.QuerySingleOrDefaultAsync<FilaContexto>(new CommandDefinition(
                "dbo.ObtenerContextoUsuarioAutorizado", new { id_usuario = idUsuario, id_empresa = idEmpresa },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            if (fila == null) return null;
            fila.Roles = JsonSerializer.Deserialize<List<RolUsuarioAutorizado>>(fila.RolesJson)
                ?? throw new InvalidOperationException("Contrato de roles invalido.");
            return fila;
        }

        private sealed class FilaContexto : ContextoUsuarioAutorizado
        {
            public string RolesJson { get; set; } = null!;
        }
    }
}