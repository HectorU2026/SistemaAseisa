namespace Abstracciones.Modelos;

public class CatalogosPagoContactoResponse
{
    public IEnumerable<TerminoPagoResponse> TerminosPago { get; set; } = Array.Empty<TerminoPagoResponse>();
    public IEnumerable<MetodoPagoResponse> MetodosPago { get; set; } = Array.Empty<MetodoPagoResponse>();
}
