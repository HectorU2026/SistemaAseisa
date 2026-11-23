namespace Abstracciones.Modelos;

public class FlujoFacturaFiltro
{
    public int? IdCotizacion { get; set; }
    public string? NumeroFactura { get; set; }
    public string? OrdenarPor { get; set; }
}

public class FlujoFacturaListado
{
    public int IdCotizacion { get; set; }
    public string NumeroCotizacion { get; set; } = string.Empty;
    public string EstadoCotizacion { get; set; } = string.Empty;
    public decimal TotalOrden { get; set; }
    public decimal TotalFacturado { get; set; }
    public decimal TotalCobradoOrden { get; set; }
    public decimal SaldoPendienteOrden { get; set; }
    public int IdVenta { get; set; }
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string EstadoFactura { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal TotalFactura { get; set; }
    public decimal SaldoPendienteFactura { get; set; }
    public int CantidadFacturasVenta { get; set; }
    public decimal MontoCobradoVenta { get; set; }
    public decimal? MontoCobradoFactura { get; set; }
}

public class FlujoFacturaConsulta
{
    public string? Mensaje { get; set; }
    public List<FlujoFacturaListado> Items { get; set; } = new();
}
