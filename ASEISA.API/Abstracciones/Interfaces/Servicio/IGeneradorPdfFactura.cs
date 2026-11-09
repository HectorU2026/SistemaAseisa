using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IGeneradorPdfFactura
    {
        byte[] Generar(FacturaDetalle factura, Empresa? empresa);
    }
}
