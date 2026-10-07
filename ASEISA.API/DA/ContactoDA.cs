using System.Data;
using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DA;

public class ContactoDA : IContactoDA
{
    private readonly string _connectionString;

    public ContactoDA(IRepositorioDapper repositorioDapper)
    {
        _connectionString = repositorioDapper.ObtenerRepositorio().ConnectionString;
    }

    public async Task<ContactoCreadoResponse> Agregar(int idEmpresa, int idUsuario, ContactoRequest contacto)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleAsync<ContactoCreadoResponse>(
            "dbo.AgregarContacto", new
            {
                id_empresa = idEmpresa,
                id_usuario = idUsuario,
                cedula = contacto.Cedula,
                identificacion_fiscal = contacto.IdentificacionFiscal,
                nombre = contacto.Nombre,
                correo = contacto.Correo,
                telefono = contacto.Telefono,
                pais = contacto.Pais,
                provincia = contacto.Provincia,
                canton = contacto.Canton,
                distrito = contacto.Distrito,
                detalle_direccion = contacto.DetalleDireccion,
                rol_contacto = contacto.RolContacto,
                id_estado = contacto.IdEstado,
                id_vendedor = contacto.IdVendedor,
                id_comprador = contacto.IdComprador,
                id_termino_pago_ventas = contacto.IdTerminoPagoVentas,
                id_metodo_pago_ventas = contacto.IdMetodoPagoVentas,
                id_termino_pago_compras = contacto.IdTerminoPagoCompras,
                id_metodo_pago_compras = contacto.IdMetodoPagoCompras
            }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ContactoResponse>> Obtener(int idEmpresa, ContactoFiltro filtro)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QueryAsync<ContactoResponse>("dbo.ObtenerContactos", new
        {
            id_empresa = idEmpresa,
            busqueda = filtro.Busqueda,
            rol_contacto = filtro.RolContacto,
            id_estado = filtro.IdEstado
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<ContactoResponse?> Obtener(int idEmpresa, int idContacto)
    {
        using var conexion = new SqlConnection(_connectionString);
        return await conexion.QuerySingleOrDefaultAsync<ContactoResponse>("dbo.ObtenerContacto", new
        {
            id_empresa = idEmpresa,
            id_contacto = idContacto
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<CatalogosPagoContactoResponse> ObtenerCatalogosPago(int idEmpresa)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var resultados = await conexion.QueryMultipleAsync("dbo.ObtenerCatalogosPagoContacto",
            new { id_empresa = idEmpresa }, commandType: CommandType.StoredProcedure);
        var terminos = (await resultados.ReadAsync<TerminoPagoResponse>()).ToList();
        var metodos = (await resultados.ReadAsync<MetodoPagoResponse>()).ToList();
        return new CatalogosPagoContactoResponse { TerminosPago = terminos, MetodosPago = metodos };
    }
}

