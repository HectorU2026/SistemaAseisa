namespace Abstracciones.Modelos;

public class FacturaFiltro
{
    public int? IdEstadoFactura { get; set; }
    public string? NombreCliente { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime? FechaEmisionDesde { get; set; }
    public DateTime? FechaEmisionHasta { get; set; }
}

public class EstadoFactura
{
    public int IdEstadoFactura { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}

public class FacturaListado
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string? CondicionVenta { get; set; }
    public int IdEstadoFactura { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? IdTerminoPago { get; set; }
    public string? TerminoPagoNombre { get; set; }
    public decimal Total { get; set; }
}
