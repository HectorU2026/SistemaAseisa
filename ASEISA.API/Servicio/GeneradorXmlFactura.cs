using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;

namespace Servicio
{
    public class GeneradorXmlFactura : IGeneradorXmlFactura
    {
        private const string NamespaceInterno = "https://aseisa.local/factura-interna";
        private const string VersionEsquema = "Aseisa-FacturaInterna-1.0";
        private const string AvisoNoTributario =
            "Este archivo no es un comprobante electrónico ante la DGT ni cumple la versión 4.4.";

        public byte[] Generar(FacturaDetalle factura, Empresa? empresa)
        {
            ArgumentNullException.ThrowIfNull(factura);

            XNamespace ns = NamespaceInterno;

            var documento = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "AseisaFacturaInterna",
                    new XAttribute("esquema", VersionEsquema),
                    new XAttribute("proposito", "academico-no-tributario"),
                    new XElement(ns + "Aviso", AvisoNoTributario),
                    CrearFactura(ns, factura),
                    CrearEmisor(ns, empresa),
                    CrearCliente(ns, factura),
                    CrearLineas(ns, factura),
                    CrearTotales(ns, factura)));

            return SerializarUtf8(documento);
        }

        private static XElement CrearFactura(XNamespace ns, FacturaDetalle factura)
        {
            var facturaXml = new XElement(ns + "Factura",
                new XElement(ns + "IdFactura", factura.IdFactura.ToString(CultureInfo.InvariantCulture)),
                new XElement(ns + "NumeroFactura", Valor(factura.NumeroFactura)),
                new XElement(ns + "Estado", Valor(factura.Estado)),
                new XElement(ns + "FechaEmision", FormatearFecha(factura.FechaEmision)),
                new XElement(ns + "FechaVencimiento", factura.FechaVencimiento.HasValue
                    ? FormatearFecha(factura.FechaVencimiento.Value)
                    : string.Empty),
                new XElement(ns + "TipoFactura", Valor(factura.TipoFactura)),
                new XElement(ns + "CondicionVenta", Valor(factura.CondicionVenta)),
                new XElement(ns + "TerminoPago", Valor(factura.TerminoPagoNombre)),
                new XElement(ns + "Moneda", Valor(factura.Moneda)),
                new XElement(ns + "TipoCambio", factura.TipoCambio.HasValue
                    ? FormatearDecimal(factura.TipoCambio.Value)
                    : string.Empty));

            return facturaXml;
        }

        private static XElement CrearEmisor(XNamespace ns, Empresa? empresa)
        {
            return new XElement(ns + "Emisor",
                new XElement(ns + "NombreComercial", Valor(empresa?.NombreComercial)),
                new XElement(ns + "RazonSocial", Valor(empresa?.RazonSocial)),
                new XElement(ns + "CedulaJuridica", Valor(empresa?.CedulaJuridica)),
                new XElement(ns + "Correo", Valor(empresa?.Correo)),
                new XElement(ns + "Telefono", Valor(empresa?.Telefono)),
                new XElement(ns + "Direccion", Valor(empresa?.Direccion)));
        }

        private static XElement CrearCliente(XNamespace ns, FacturaDetalle factura)
        {
            return new XElement(ns + "Cliente",
                new XElement(ns + "Nombre", Valor(factura.NombreCliente)),
                new XElement(ns + "Identificacion", Valor(factura.Identificacion)),
                new XElement(ns + "Correo", Valor(factura.CorreoCliente)));
        }

        private static XElement CrearLineas(XNamespace ns, FacturaDetalle factura)
        {
            var lineas = new XElement(ns + "Lineas");

            foreach (var producto in factura.Productos ?? new List<FacturaProducto>())
            {
                lineas.Add(new XElement(ns + "Linea",
                    new XElement(ns + "Codigo", Valor(producto.Codigo)),
                    new XElement(ns + "Nombre", Valor(producto.Nombre)),
                    new XElement(ns + "Cantidad", FormatearDecimal(producto.Cantidad)),
                    new XElement(ns + "PrecioUnitario", FormatearDecimal(producto.PrecioUnitario)),
                    new XElement(ns + "PorcentajeDescuento", producto.PorcentajeDescuento.HasValue
                        ? FormatearDecimal(producto.PorcentajeDescuento.Value)
                        : string.Empty),
                    new XElement(ns + "PorcentajeImpuesto", producto.PorcentajeImpuesto.HasValue
                        ? FormatearDecimal(producto.PorcentajeImpuesto.Value)
                        : string.Empty),
                    new XElement(ns + "Subtotal", FormatearDecimal(producto.Subtotal))));
            }

            return lineas;
        }

        private static XElement CrearTotales(XNamespace ns, FacturaDetalle factura)
        {
            return new XElement(ns + "Totales",
                new XElement(ns + "Subtotal", FormatearDecimal(factura.Subtotal)),
                new XElement(ns + "Descuento", FormatearDecimal(factura.Descuento)),
                new XElement(ns + "Impuesto", FormatearDecimal(factura.Impuesto)),
                new XElement(ns + "Total", FormatearDecimal(factura.Total)),
                new XElement(ns + "SaldoPendiente", FormatearDecimal(factura.SaldoPendiente)));
        }

        private static string Valor(string? texto) => texto ?? string.Empty;

        private static string FormatearFecha(DateTime fecha) =>
            fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        private static string FormatearDecimal(decimal valor) =>
            valor.ToString("0.############################", CultureInfo.InvariantCulture);

        private static byte[] SerializarUtf8(XDocument documento)
        {
            using var stream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                documento.Save(writer);
            }

            return stream.ToArray();
        }
    }
}
