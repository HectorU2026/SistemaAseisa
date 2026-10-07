using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA;

public interface IContactoDA
{
    Task<ContactoCreadoResponse> Agregar(int idEmpresa, int idUsuario, ContactoRequest contacto);
    Task<IEnumerable<ContactoResponse>> Obtener(int idEmpresa, ContactoFiltro filtro);
    Task<ContactoResponse?> Obtener(int idEmpresa, int idContacto);
    Task<CatalogosPagoContactoResponse> ObtenerCatalogosPago(int idEmpresa);
}
