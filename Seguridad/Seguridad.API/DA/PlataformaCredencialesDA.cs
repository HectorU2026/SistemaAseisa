using ASEISA.Plataforma;
using Abstracciones.DA;
using Dapper;
using System.Data;
using System.Data.SqlClient;
namespace DA;
public sealed class PlataformaCredencialesDA:IPlataformaCredencialesDA
{
    private readonly IRepositorioDapper repositorio;
    public PlataformaCredencialesDA(IRepositorioDapper repositorio){this.repositorio=repositorio;}
    private SqlConnection Conexion()=>new(repositorio.ObtenerRepositorioDapper().ConnectionString);
    private sealed class Catalogo {public int IdRol{get;set;}public int IdEstado{get;set;}}
    public async Task<(int Rol,int Estado)> Validar(int actor,int empresa,int? usuario,CancellationToken ct)
    {
        using var c=Conexion();var valor=await c.QuerySingleAsync<Catalogo>(new CommandDefinition("dbo.ValidarAdministradorPlataforma",new{actor,empresa,usuario},commandType:CommandType.StoredProcedure,cancellationToken:ct));
        return(valor.IdRol,valor.IdEstado);
    }
    public async Task<int> Inicializar(PersonaPlataforma p,string usuario,string hash,byte[] resumen,string cifrado,string destino,CancellationToken ct)
    {
        using var c=Conexion();await c.OpenAsync(ct);
        var identidad=await c.QuerySingleAsync<string>("SELECT CONVERT(NVARCHAR(128),SERVERPROPERTY('ServerName'))+'/'+DB_NAME()");
        if(!string.Equals(identidad,destino,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("El destino confirmado no coincide con la conexion efectiva.");
        using var tx=c.BeginTransaction();
        var id=await c.QuerySingleAsync<int>(new CommandDefinition("dbo.InicializarSuperadministrador",new{nombre=p.Nombre.Trim(),primer_apellido=p.PrimerApellido.Trim(),segundo_apellido=p.SegundoApellido?.Trim(),correo=p.Correo.Trim(),nombre_usuario=usuario,contrasena_hash=hash},tx,commandType:CommandType.StoredProcedure,cancellationToken:ct));
        var invitacion=await c.QuerySingleAsync<Abstracciones.Modelos.DestinatarioContrasena>(new CommandDefinition("dbo.CrearSolicitudContrasena",new{hash_token=resumen,token_cifrado=cifrado,proposito="Invitacion",id_usuario=id,id_empresa=(int?)null,id_usuario_accion=id},tx,commandType:CommandType.StoredProcedure,cancellationToken:ct));
        tx.Commit();return id;
    }
}