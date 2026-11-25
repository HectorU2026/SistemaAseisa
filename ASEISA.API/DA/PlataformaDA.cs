using Abstracciones.Interfaces.DA;
using ASEISA.Plataforma;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
namespace DA;
public sealed class PlataformaDA:IPlataformaDA
{
    private readonly IRepositorioDapper repositorio;
    public PlataformaDA(IRepositorioDapper repositorio){this.repositorio=repositorio;}
    private SqlConnection Conexion()=>new(repositorio.ObtenerRepositorio().ConnectionString);
    public async Task<VistaPlataforma> Consultar(int actor,int? empresa,CancellationToken ct)
    {
        using var c=Conexion();using var datos=await c.QueryMultipleAsync(new CommandDefinition("dbo.ConsultarPlataforma",new{actor,empresa},commandType:CommandType.StoredProcedure,cancellationToken:ct));
        return new(){Empresas=(await datos.ReadAsync<EmpresaPlataforma>()).ToList(),Administradores=(await datos.ReadAsync<AdministradorPlataforma>()).ToList(),Eventos=(await datos.ReadAsync<EventoPlataforma>()).ToList()};
    }
    public async Task<int> EstadoEmpresa(int actor,int empresa,bool activo,CancellationToken ct)
    {
        using var c=Conexion();return await c.QuerySingleAsync<int>(new CommandDefinition("dbo.CambiarEstadoEmpresaPlataforma",new{actor,empresa,activo},commandType:CommandType.StoredProcedure,cancellationToken:ct));
    }
    public async Task<int> Guardar(int actor,int? empresa,EmpresaPlataforma datos,CancellationToken ct)
    {
        using var c=Conexion();return await c.QuerySingleAsync<int>(new CommandDefinition("dbo.GuardarEmpresaPlataforma",new{actor,empresa,
            razon_social=datos.RazonSocial.Trim(),nombre_comercial=datos.NombreComercial?.Trim(),cedula_juridica=datos.CedulaJuridica,
            correo=datos.Correo?.Trim(),telefono=ASEISA.Usuarios.ReglasRegistroUsuario.NormalizarTelefono(datos.Telefono),direccion=datos.Direccion?.Trim(),moneda_base=datos.MonedaBase.Trim().ToUpperInvariant(),activo=datos.Activo},commandType:CommandType.StoredProcedure,cancellationToken:ct));
    }
    public async Task<int> EditarAdministrador(int actor,int empresa,int usuario,EdicionAdministradorPlataforma datos,CancellationToken ct)
    {
        using var c=Conexion();return await c.QuerySingleAsync<int>(new CommandDefinition("dbo.GestionarAdministradorPlataforma",new{actor,empresa,usuario,operacion="Editar",nombre=datos.Nombre.Trim(),primer_apellido=datos.PrimerApellido.Trim(),segundo_apellido=datos.SegundoApellido?.Trim(),correo=datos.Correo.Trim()},commandType:CommandType.StoredProcedure,cancellationToken:ct));
    }
    public async Task<int> EstadoAdministrador(int actor,int empresa,int usuario,bool activo,CancellationToken ct)
    {
        using var c=Conexion();return await c.QuerySingleAsync<int>(new CommandDefinition("dbo.GestionarAdministradorPlataforma",new{actor,empresa,usuario,operacion="Estado",activo},commandType:CommandType.StoredProcedure,cancellationToken:ct));
    }
}