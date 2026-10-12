namespace Abstracciones.Modelos;

public class TerminoPagoResponse
{
    public int IdTerminoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int DiasCredito { get; set; }
}

public class MetodoPagoResponse
{
    public int IdMetodoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogosPagoContactoResponse
{
    public IEnumerable<TerminoPagoResponse> TerminosPago { get; set; } = Array.Empty<TerminoPagoResponse>();
    public IEnumerable<MetodoPagoResponse> MetodosPago { get; set; } = Array.Empty<MetodoPagoResponse>();
}
