using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.DA;

public interface ICompraDA
{
    Task<CompraGuardadaResponse> Agregar(int idEmpresa, int idUsuario, CompraRequest compra);
    Task<CompraGuardadaResponse> Editar(int idEmpresa, int idUsuario, int idCompra, CompraRequest compra);
    Task<int> Anular(int idEmpresa, int idUsuario, int idCompra);
    Task<CompraPaginaResponse> Obtener(int idEmpresa, int idUsuario, CompraFiltro filtro);
    Task<CompraResponse?> Obtener(int idEmpresa, int idUsuario, int idCompra);
    Task<CompraCatalogosResponse> ObtenerCatalogos(int idEmpresa, int idUsuario);
}
