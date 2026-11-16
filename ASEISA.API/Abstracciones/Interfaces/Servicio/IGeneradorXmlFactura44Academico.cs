using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.Servicio
{
    public interface IGeneradorXmlFactura44Academico
    {
        byte[] Generar(FacturaDetalle factura, Empresa? empresa);
    }
}
