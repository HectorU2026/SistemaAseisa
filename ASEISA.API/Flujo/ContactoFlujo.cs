using System.ComponentModel.DataAnnotations;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo;

public class ContactoFlujo : IContactoFlujo
{
    private readonly IContactoDA _contactoDA;

    public ContactoFlujo(IContactoDA contactoDA)
    {
        _contactoDA = contactoDA;
    }

    public async Task<ContactoCreadoResponse> Agregar(int idEmpresa, int idUsuario, ContactoRequest contacto)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ArgumentNullException.ThrowIfNull(contacto);

        var datos = new ContactoRequest
        {
            Cedula = Limpiar(contacto.Cedula),
            IdentificacionFiscal = Limpiar(contacto.IdentificacionFiscal),
            Nombre = Limpiar(contacto.Nombre) ?? string.Empty,
            Correo = Limpiar(contacto.Correo) ?? string.Empty,
            Telefono = Limpiar(contacto.Telefono) ?? string.Empty,
            Pais = Limpiar(contacto.Pais) ?? string.Empty,
            Provincia = Limpiar(contacto.Provincia),
            Canton = Limpiar(contacto.Canton),
            Distrito = Limpiar(contacto.Distrito),
            DetalleDireccion = Limpiar(contacto.DetalleDireccion) ?? string.Empty,
            RolContacto = Limpiar(contacto.RolContacto) ?? string.Empty,
            IdEstado = contacto.IdEstado,
            IdVendedor = contacto.IdVendedor,
            IdComprador = contacto.IdComprador,
            IdTerminoPagoVentas = contacto.IdTerminoPagoVentas,
            IdMetodoPagoVentas = contacto.IdMetodoPagoVentas,
            IdTerminoPagoCompras = contacto.IdTerminoPagoCompras,
            IdMetodoPagoCompras = contacto.IdMetodoPagoCompras
        };
        if (string.Equals(datos.Pais, "Costa Rica", StringComparison.OrdinalIgnoreCase))
            datos.Pais = "Costa Rica";
        ValidarModelo(datos);
        return await _contactoDA.Agregar(idEmpresa, idUsuario, datos);
    }

    public async Task<int> Editar(int idEmpresa, int idUsuario, int idContacto, ContactoRequest contacto)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ValidarId(idContacto, nameof(idContacto));
        ArgumentNullException.ThrowIfNull(contacto);
        var datos = NormalizarYValidar(contacto);
        return await _contactoDA.Editar(idEmpresa, idUsuario, idContacto, datos);
    }

    public async Task<IEnumerable<ContactoResponse>> Obtener(int idEmpresa, ContactoFiltro filtro)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ArgumentNullException.ThrowIfNull(filtro);
        var datos = new ContactoFiltro
        {
            Busqueda = Limpiar(filtro.Busqueda),
            RolContacto = Limpiar(filtro.RolContacto),
            IdEstado = filtro.IdEstado
        };
        ValidarModelo(datos);
        return await _contactoDA.Obtener(idEmpresa, datos);
    }

    public async Task<ContactoResponse?> Obtener(int idEmpresa, int idContacto)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idContacto, nameof(idContacto));
        return await _contactoDA.Obtener(idEmpresa, idContacto);
    }

    public async Task<CatalogosPagoContactoResponse> ObtenerCatalogosPago(int idEmpresa)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        return await _contactoDA.ObtenerCatalogosPago(idEmpresa);
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static void ValidarId(int valor, string nombre)
    {
        if (valor <= 0)
            throw new ValidationException($"{nombre} debe ser mayor que cero.");
    }

    private static void ValidarModelo(object modelo)
    {
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true))
            throw new ValidationException(string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }

    private static ContactoRequest NormalizarYValidar(ContactoRequest contacto)
    {
        var datos = new ContactoRequest
        {
            Cedula = Limpiar(contacto.Cedula),
            IdentificacionFiscal = Limpiar(contacto.IdentificacionFiscal),
            Nombre = Limpiar(contacto.Nombre) ?? string.Empty,
            Correo = Limpiar(contacto.Correo) ?? string.Empty,
            Telefono = Limpiar(contacto.Telefono) ?? string.Empty,
            Pais = Limpiar(contacto.Pais) ?? string.Empty,
            Provincia = Limpiar(contacto.Provincia),
            Canton = Limpiar(contacto.Canton),
            Distrito = Limpiar(contacto.Distrito),
            DetalleDireccion = Limpiar(contacto.DetalleDireccion) ?? string.Empty,
            RolContacto = Limpiar(contacto.RolContacto) ?? string.Empty,
            IdEstado = contacto.IdEstado,
            IdVendedor = contacto.IdVendedor,
            IdComprador = contacto.IdComprador,
            IdTerminoPagoVentas = contacto.IdTerminoPagoVentas,
            IdMetodoPagoVentas = contacto.IdMetodoPagoVentas,
            IdTerminoPagoCompras = contacto.IdTerminoPagoCompras,
            IdMetodoPagoCompras = contacto.IdMetodoPagoCompras
        };
        if (string.Equals(datos.Pais, "Costa Rica", StringComparison.OrdinalIgnoreCase))
            datos.Pais = "Costa Rica";
        ValidarModelo(datos);
        return datos;
    }
}
