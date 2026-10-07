using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA;

public interface IContactoDA
{
    Task<ContactoCreadoResponse> Agregar(int idEmpresa, int idUsuario, ContactoRequest contacto);
    Task<int> Editar(int idEmpresa, int idUsuario, int idContacto, ContactoRequest contacto);
    Task<IEnumerable<ContactoResponse>> Obtener(int idEmpresa, ContactoFiltro filtro);
    Task<ContactoResponse?> Obtener(int idEmpresa, int idContacto, int idUsuario);
    Task<CatalogosPagoContactoResponse> ObtenerCatalogosPago(int idEmpresa, int idUsuario);
    Task<int> CambiarEstado(int idEmpresa, int idUsuario, int idContacto, bool activo);
}
