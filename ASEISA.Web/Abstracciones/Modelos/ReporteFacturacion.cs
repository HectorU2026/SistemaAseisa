namespace Abstracciones.Modelos;

public class ReporteFacturacionFiltro
{
    public string? Periodo { get; set; }
    public string? TipoFactura { get; set; }
    public string? OrdenarPor { get; set; }
}

public class ReporteFacturacionLinea
{
    public string Periodo { get; set; } = string.Empty;
    public string? TipoFactura { get; set; }
    public string? Moneda { get; set; }
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal TotalNotasCreditoFactura { get; set; }
}

public class ReporteFacturacionGrupo
{
    public string Periodo { get; set; } = string.Empty;
    public string? TipoFactura { get; set; }
    public string? Moneda { get; set; }
    public int CantidadFacturas { get; set; }
    public decimal TotalFacturado { get; set; }
    public decimal TotalNotasCredito { get; set; }
    public List<ReporteFacturacionLinea> Facturas { get; set; } = new();
}

public class ReporteFacturacionConsulta
{
    public string? Mensaje { get; set; }
    public List<ReporteFacturacionGrupo> Grupos { get; set; } = new();
}
