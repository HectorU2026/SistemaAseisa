using Abstracciones.DA;
using Abstracciones.Modelos;
using Dapper;
using System.Data;
using System.Data.SqlClient;
namespace DA
{
    public sealed class CredencialesDA : ICredencialesDA
    {
        private readonly IRepositorioDapper _repositorio;
        public CredencialesDA(IRepositorioDapper repositorio) { _repositorio=repositorio; }
        private SqlConnection Conexion()=>new(_repositorio.ObtenerRepositorioDapper().ConnectionString);
        public async Task<(int IdUsuario,DestinatarioContrasena? Destinatario)> Registrar(RegistroAdministrativoRequest s,int empresa,int actor,string hash,byte[] hashToken,string tokenCifrado,CancellationToken ct)
        {
            using var c=Conexion(); await c.OpenAsync(ct); using var tx=c.BeginTransaction();
            var id=await c.QuerySingleAsync<int>(new CommandDefinition("dbo.RegistroUsuario",new {
                id_empresa=empresa,tipo_cliente=s.TipoCliente,nombre=s.Nombre.Trim(),primer_apellido=s.PrimerApellido.Trim(),
                segundo_apellido=s.SegundoApellido?.Trim(),correo=s.Correo.Trim(),nombre_usuario=s.NombreUsuario,contrasena_hash=hash,
                id_estado=s.IdEstado,telefono=ASEISA.Usuarios.ReglasRegistroUsuario.NormalizarTelefono(s.Telefono),direccion=s.Direccion?.Trim(),identificacion=ASEISA.Usuarios.ReglasRegistroUsuario.NormalizarIdentificacion(s.Identificacion),
                nombre_razon_social=string.IsNullOrWhiteSpace(s.NombreRazonSocial)?null:s.NombreRazonSocial.Trim(),id_rol=s.IdRol,roles_json=s.IdsRoles?.Count>0?System.Text.Json.JsonSerializer.Serialize(s.IdsRoles):null,registro_publico=false,id_usuario_accion=actor
            },tx,commandType:CommandType.StoredProcedure,cancellationToken:ct));
            var activo=await c.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.USUARIO U INNER JOIN dbo.ESTADO E ON E.id_estado=U.id_estado WHERE U.id_usuario=@id AND LTRIM(RTRIM(E.nombre_estado))='Activo') THEN 1 ELSE 0 END AS BIT)",new {id},tx,cancellationToken:ct));
            DestinatarioContrasena? destino=null;
            if(activo) destino=await c.QuerySingleOrDefaultAsync<DestinatarioContrasena>(new CommandDefinition(
                "dbo.CrearSolicitudContrasena",new {hash_token=hashToken,token_cifrado=tokenCifrado,proposito="Invitacion",id_usuario=id,id_empresa=empresa,id_usuario_accion=actor},tx,commandType:CommandType.StoredProcedure,cancellationToken:ct));
            tx.Commit(); return (id,destino);
        }
        public async Task<DestinatarioContrasena?> Solicitar(byte[] hashToken,string tokenCifrado,string proposito,string? correo,int? usuario,int? empresa,int? actor,CancellationToken ct)
        {
            using var c=Conexion();
            return await c.QuerySingleOrDefaultAsync<DestinatarioContrasena>(new CommandDefinition("dbo.CrearSolicitudContrasena",
                new {hash_token=hashToken,token_cifrado=tokenCifrado,proposito,correo,id_usuario=usuario,id_empresa=empresa,id_usuario_accion=actor},commandType:CommandType.StoredProcedure,cancellationToken:ct));
        }
        public async Task<bool> Consumir(byte[] hashToken,string hashContrasena,CancellationToken ct)
        {
            using var c=Conexion();return await c.QuerySingleAsync<bool>(new CommandDefinition("dbo.ConsumirSolicitudContrasena",
                new {hash_token=hashToken,contrasena_hash=hashContrasena},commandType:CommandType.StoredProcedure,cancellationToken:ct));
        }
    }
}