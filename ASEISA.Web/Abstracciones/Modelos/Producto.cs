namespace Abstracciones.Modelos;

public class ProductoFiltro
{
    public int? IdEmpresa { get; set; }
    public string? Nombre { get; set; }
    public int? Pagina { get; set; }
    public int? TamanoPagina { get; set; }
}

public class ProductoCatalogoItem
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoProducto { get; set; } = string.Empty;
    public string PoliticaFacturacion { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal PorcentajeImpuestoVenta { get; set; }
    public decimal? CostoPromedio { get; set; }
    public decimal PorcentajeImpuestoCompra { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Peso { get; set; }
    public decimal? Volumen { get; set; }
    public int TiempoEntrega { get; set; }
    public int IdCuentaIngreso { get; set; }
    public string CuentaIngreso { get; set; } = string.Empty;
    public int IdCuentaGasto { get; set; }
    public string CuentaGasto { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class ProductoCatalogoRespuesta
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int Total { get; set; }
    public List<ProductoCatalogoItem> Items { get; set; } = new();
}
