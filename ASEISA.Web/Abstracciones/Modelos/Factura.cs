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

public class FacturaProducto
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? PorcentajeDescuento { get; set; }
    public decimal? PorcentajeImpuesto { get; set; }
    public decimal Subtotal { get; set; }
}

public class FacturaMovimientoContable
{
    public int IdAsiento { get; set; }
    public int IdCuentaContable { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

public class FacturaAsiento
{
    public int IdAsiento { get; set; }
    public string NumeroAsiento { get; set; } = string.Empty;
    public DateTime FechaAsiento { get; set; }
    public string? Descripcion { get; set; }
    public string? TipoOrigen { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<FacturaMovimientoContable> Movimientos { get; set; } = new();
}

public class FacturaDetalle
{
    public int IdFactura { get; set; }
    public int IdEmpresa { get; set; }
    public int IdVenta { get; set; }
    public int IdUsuario { get; set; }
    public int IdCliente { get; set; }
    public int? IdCotizacion { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string? CorreoCliente { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public string? CondicionVenta { get; set; }
    public string? TipoFactura { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? IdTerminoPago { get; set; }
    public string? TerminoPagoNombre { get; set; }
    public string? Moneda { get; set; }
    public decimal? TipoCambio { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? MotivoAnulacion { get; set; }
    public int IdEstadoFactura { get; set; }
    public string Estado { get; set; } = string.Empty;
    public byte[]? VersionFila { get; set; }
    public List<FacturaProducto> Productos { get; set; } = new();
    public FacturaAsiento? Asiento { get; set; }
}

public class FacturaCambioEstadoRequest
{
    public byte[] VersionFila { get; set; } = Array.Empty<byte>();
}

public class FacturaCambioEstadoRespuesta
{
    public int? IdFactura { get; set; }
    public string? NumeroFactura { get; set; }
    public string? Estado { get; set; }
    public byte[]? VersionFila { get; set; }
    public string? Mensaje { get; set; }
}
