using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IGeneradorXmlFactura
    {
        byte[] Generar(FacturaDetalle factura, Empresa? empresa);
    }
}
