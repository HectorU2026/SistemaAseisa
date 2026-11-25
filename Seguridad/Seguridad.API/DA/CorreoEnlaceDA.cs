using Abstracciones.DA;
using Abstracciones.Modelos;
using Dapper;
using System.Data;
using System.Data.SqlClient;
namespace DA
{
    public sealed class CorreoEnlaceDA:ICorreoEnlaceDA
    {
        private readonly IRepositorioDapper _repositorio;
        public CorreoEnlaceDA(IRepositorioDapper repositorio){_repositorio=repositorio;}
        public async Task<CorreoEnlacePendiente?> Tomar(CancellationToken ct)
        {
            using var c=new SqlConnection(_repositorio.ObtenerRepositorioDapper().ConnectionString);
            return await c.QuerySingleOrDefaultAsync<CorreoEnlacePendiente>(new CommandDefinition("dbo.TomarCorreoEnlace",commandType:CommandType.StoredProcedure,cancellationToken:ct));
        }
        public async Task Finalizar(long id,Guid reserva,bool enviado,CancellationToken ct)
        {
            using var c=new SqlConnection(_repositorio.ObtenerRepositorioDapper().ConnectionString);
            await c.ExecuteAsync(new CommandDefinition("dbo.FinalizarCorreoEnlace",new{id_correo=id,reserva,enviado},commandType:CommandType.StoredProcedure,cancellationToken:ct));
        }
    }
}