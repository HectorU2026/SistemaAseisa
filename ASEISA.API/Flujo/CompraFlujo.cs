using System.ComponentModel.DataAnnotations;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo;

public class CompraFlujo : ICompraFlujo
{
    private readonly ICompraDA _compraDA;

    public CompraFlujo(ICompraDA compraDA)
    {
        _compraDA = compraDA;
    }

    #region "Operaciones"

    public async Task<CompraGuardadaResponse> Agregar(int idEmpresa, int idUsuario, CompraRequest compra)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ValidarCompra(compra);

        return await _compraDA.Agregar(idEmpresa, idUsuario, compra);
    }

    public async Task<CompraGuardadaResponse> Editar(int idEmpresa, int idUsuario, int idCompra, CompraRequest compra)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ValidarId(idCompra, nameof(idCompra));
        ValidarCompra(compra);

        return await _compraDA.Editar(idEmpresa, idUsuario, idCompra, compra);
    }

    public async Task<int> Anular(int idEmpresa, int idUsuario, int idCompra)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ValidarId(idCompra, nameof(idCompra));

        return await _compraDA.Anular(idEmpresa, idUsuario, idCompra);
    }

    public async Task<CompraPaginaResponse> Obtener(int idEmpresa, int idUsuario, CompraFiltro filtro)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ArgumentNullException.ThrowIfNull(filtro);

        var datos = new CompraFiltro
        {
            Busqueda = Limpiar(filtro.Busqueda),
            IdEstado = filtro.IdEstado,
            Pagina = filtro.Pagina,
            TamanoPagina = filtro.TamanoPagina
        };
        ValidarModelo(datos);

        return await _compraDA.Obtener(idEmpresa, idUsuario, datos);
    }

    public async Task<CompraResponse?> Obtener(int idEmpresa, int idUsuario, int idCompra)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));
        ValidarId(idCompra, nameof(idCompra));

        return await _compraDA.Obtener(idEmpresa, idUsuario, idCompra);
    }

    public async Task<CompraCatalogosResponse> ObtenerCatalogos(int idEmpresa, int idUsuario)
    {
        ValidarId(idEmpresa, nameof(idEmpresa));
        ValidarId(idUsuario, nameof(idUsuario));

        return await _compraDA.ObtenerCatalogos(idEmpresa, idUsuario);
    }

    #endregion

    #region "Helpers"

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

    private static void ValidarCompra(CompraRequest compra)
    {
        ArgumentNullException.ThrowIfNull(compra);
        ValidarModelo(compra);

        foreach (var producto in compra.Productos)
        {
            if (producto == null)
                throw new ValidationException("La lista no puede contener productos nulos.");

            ValidarModelo(producto);
        }
    }

    #endregion
}

