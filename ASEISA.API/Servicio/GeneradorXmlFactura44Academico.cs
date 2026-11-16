using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;

namespace Servicio
{
    public class GeneradorXmlFactura44Academico : IGeneradorXmlFactura44Academico
    {
        public const string NamespaceAcademico = "https://aseisa.local/fe-44-academica";
        public const string VersionEsquema = "ASEISA-FE44-Academica-1.0";
        public const string Proposito = "academico-no-tributario";
        public const string EstadoRepresentado = "Emitida";
        public const string AvisoNoTributario =
            "Documento académico de simulación inspirado en facturación electrónica 4.4. No es un comprobante electrónico válido ante el Ministerio de Hacienda ni ante la DGT.";

        public byte[] Generar(FacturaDetalle factura, Empresa? empresa)
        {
            ArgumentNullException.ThrowIfNull(factura);

            XNamespace ns = NamespaceAcademico;

            var documento = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "FacturaElectronicaAcademica",
                    new XAttribute("version", VersionEsquema),
                    CrearIdentificacion(ns, factura),
                    CrearEmisor(ns, factura, empresa),
                    CrearReceptor(ns, factura),
                    CrearDetalle(ns, factura),
                    CrearResumen(ns, factura),
                    CrearCamposNoDisponibles(ns, CamposDocumentoNoDisponibles())));

            return SerializarUtf8(documento);
        }

        private static XElement CrearIdentificacion(XNamespace ns, FacturaDetalle factura)
        {
            return new XElement(ns + "IdentificacionDocumento",
                new XElement(ns + "IdentificadorAcademico", IdentificadorAcademico(factura)),
                new XElement(ns + "NumeroInterno", Valor(factura.NumeroFactura)),
                new XElement(ns + "FechaEmision", FormatearFecha(factura.FechaEmision)),
                new XElement(ns + "EstadoRepresentado", EstadoRepresentado),
                new XElement(ns + "TipoDocumentoInterno", Valor(factura.TipoFactura)),
                new XElement(ns + "CondicionVentaInterna", Valor(factura.CondicionVenta)),
                new XElement(ns + "Proposito", Proposito),
                new XElement(ns + "ValidezTributaria", "false"),
                new XElement(ns + "Aviso", AvisoNoTributario));
        }

        private static XElement CrearEmisor(XNamespace ns, FacturaDetalle factura, Empresa? empresa)
        {
            return new XElement(ns + "Emisor",
                new XElement(ns + "IdEmpresa", factura.IdEmpresa.ToString(CultureInfo.InvariantCulture)),
                new XElement(ns + "NombreComercial", Valor(empresa?.NombreComercial)),
                new XElement(ns + "Nombre", Valor(empresa?.RazonSocial)),
                new XElement(ns + "Identificacion", Valor(empresa?.CedulaJuridica)),
                new XElement(ns + "Correo", Valor(empresa?.Correo)),
                new XElement(ns + "Telefono", Valor(empresa?.Telefono)),
                new XElement(ns + "Direccion", Valor(empresa?.Direccion)),
                new XElement(ns + "MonedaBase", Valor(empresa?.MonedaBase)),
                CrearCamposNoDisponibles(ns, CamposEmisorNoDisponibles(empresa)));
        }

        private static XElement CrearReceptor(XNamespace ns, FacturaDetalle factura)
        {
            return new XElement(ns + "Receptor",
                new XElement(ns + "IdCliente", factura.IdCliente.ToString(CultureInfo.InvariantCulture)),
                new XElement(ns + "Nombre", Valor(factura.NombreCliente)),
                new XElement(ns + "Identificacion", Valor(factura.Identificacion)),
                new XElement(ns + "Correo", Valor(factura.CorreoCliente)),
                CrearCamposNoDisponibles(ns, CamposReceptorNoDisponibles()));
        }

        private static XElement CrearDetalle(XNamespace ns, FacturaDetalle factura)
        {
            var detalle = new XElement(ns + "DetalleServicio");
            var lineas = (factura.Productos ?? new List<FacturaProducto>())
                .OrderBy(p => p.IdProducto)
                .ThenBy(p => p.Codigo ?? string.Empty, StringComparer.Ordinal)
                .ToList();

            var numeroLinea = 1;
            foreach (var producto in lineas)
            {
                var montoBruto = producto.Cantidad * producto.PrecioUnitario;
                var montoDescuento = CalcularPorcentaje(montoBruto, producto.PorcentajeDescuento);
                var baseImponible = montoDescuento.HasValue ? montoBruto - montoDescuento.Value : montoBruto;
                var montoImpuesto = CalcularPorcentaje(baseImponible, producto.PorcentajeImpuesto);

                detalle.Add(new XElement(ns + "LineaDetalle",
                    new XElement(ns + "NumeroLinea", numeroLinea.ToString(CultureInfo.InvariantCulture)),
                    new XElement(ns + "CodigoInterno", Valor(producto.Codigo)),
                    new XElement(ns + "Detalle", Valor(producto.Nombre)),
                    new XElement(ns + "Cantidad", FormatearDecimal(producto.Cantidad)),
                    new XElement(ns + "PrecioUnitario", FormatearDecimal(producto.PrecioUnitario)),
                    new XElement(ns + "MontoBruto", FormatearDecimal(montoBruto)),
                    new XElement(ns + "PorcentajeDescuento", FormatearOpcional(producto.PorcentajeDescuento)),
                    new XElement(ns + "MontoDescuento", FormatearOpcional(montoDescuento)),
                    new XElement(ns + "PorcentajeImpuesto", FormatearOpcional(producto.PorcentajeImpuesto)),
                    new XElement(ns + "MontoImpuesto", FormatearOpcional(montoImpuesto)),
                    new XElement(ns + "SubtotalRegistrado", FormatearDecimal(producto.Subtotal)),
                    CrearCamposNoDisponibles(ns, CamposLineaNoDisponibles(producto))));

                numeroLinea++;
            }

            return detalle;
        }

        private static XElement CrearResumen(XNamespace ns, FacturaDetalle factura)
        {
            var resumen = new XElement(ns + "ResumenFactura",
                new XElement(ns + "CodigoMoneda", Valor(factura.Moneda)));

            if (factura.TipoCambio.HasValue)
                resumen.Add(new XElement(ns + "TipoCambio", FormatearDecimal(factura.TipoCambio.Value)));

            resumen.Add(
                new XElement(ns + "TotalVenta", FormatearDecimal(factura.Subtotal)),
                new XElement(ns + "TotalDescuentos", FormatearDecimal(factura.Descuento)),
                new XElement(ns + "TotalImpuesto", FormatearDecimal(factura.Impuesto)),
                new XElement(ns + "TotalComprobante", FormatearDecimal(factura.Total)),
                CrearCamposNoDisponibles(ns, CamposResumenNoDisponibles(factura)));

            return resumen;
        }

        private static XElement CrearCamposNoDisponibles(XNamespace ns, IEnumerable<(string Nombre, string Motivo)> campos)
        {
            var contenedor = new XElement(ns + "CamposNoDisponibles");
            foreach (var campo in campos)
            {
                contenedor.Add(new XElement(ns + "Campo",
                    new XAttribute("nombre", campo.Nombre),
                    new XAttribute("motivo", campo.Motivo)));
            }

            return contenedor;
        }

        private static IEnumerable<(string Nombre, string Motivo)> CamposDocumentoNoDisponibles()
        {
            yield return ("ClaveNumericaHacienda", "No se genera clave de 50 dígitos; el documento es académico y no tributario.");
            yield return ("ConsecutivoOficialHacienda", "ASEISA no almacena el consecutivo oficial de comprobantes electrónicos.");
            yield return ("XsdOficialHacienda", "Este XML no declara ni valida esquemas oficiales de la DGT.");
        }

        private static IEnumerable<(string Nombre, string Motivo)> CamposEmisorNoDisponibles(Empresa? empresa)
        {
            yield return ("CodigoActividadEconomica", "La empresa no tiene código de actividad económica de Hacienda en ASEISA.");
            yield return ("TipoIdentificacionHacienda", "No se mapea la cédula jurídica a catálogos oficiales de identificación.");
            yield return ("UbicacionAdministrativa", "No existen provincia, cantón ni distrito separados; solo dirección libre si está registrada.");

            if (empresa == null)
                yield return ("DatosEmisor", "No se recibió la empresa; no se inventan datos fiscales del emisor.");
        }

        private static IEnumerable<(string Nombre, string Motivo)> CamposReceptorNoDisponibles()
        {
            yield return ("Telefono", "ObtenerFactura no expone el teléfono de CLIENTE.");
            yield return ("Direccion", "ObtenerFactura no expone la dirección de CLIENTE.");
            yield return ("TipoCliente", "ObtenerFactura no expone tipo_cliente.");
            yield return ("TipoIdentificacionHacienda", "La identificación interna no se traduce a catálogos oficiales.");
        }

        private static IEnumerable<(string Nombre, string Motivo)> CamposLineaNoDisponibles(FacturaProducto producto)
        {
            yield return ("CodigoCABYS", "ASEISA no almacena códigos CABYS.");
            yield return ("UnidadMedidaHacienda", "No existe catálogo oficial de unidad de medida en el detalle interno.");
            yield return ("CodigoImpuestoHacienda", "El porcentaje interno no se mapea a códigos de impuesto de Hacienda.");

            if (!producto.PorcentajeDescuento.HasValue)
                yield return ("MontoDescuento", "DETALLE_FACTURA no tiene porcentaje de descuento; no se inventa el monto.");

            if (!producto.PorcentajeImpuesto.HasValue)
                yield return ("MontoImpuesto", "DETALLE_FACTURA no tiene porcentaje de impuesto; no se inventa el monto.");
        }

        private static IEnumerable<(string Nombre, string Motivo)> CamposResumenNoDisponibles(FacturaDetalle factura)
        {
            if (string.IsNullOrWhiteSpace(factura.Moneda))
                yield return ("CodigoMoneda", "FACTURA.moneda no está registrado; no se asume una moneda fiscal.");

            if (!factura.TipoCambio.HasValue)
                yield return ("TipoCambio", "FACTURA.tipo_cambio no está registrado; no se asume tipo de cambio 1.");
        }

        private static string IdentificadorAcademico(FacturaDetalle factura)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"ASEISA-FE44-ACAD-{factura.IdEmpresa}-{factura.IdFactura}");
        }

        private static decimal? CalcularPorcentaje(decimal baseCalculo, decimal? porcentaje)
        {
            if (!porcentaje.HasValue)
                return null;

            return decimal.Round(baseCalculo * porcentaje.Value / 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static string Valor(string? texto) => texto ?? string.Empty;

        private static string FormatearFecha(DateTime fecha) =>
            fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

        private static string FormatearDecimal(decimal valor) =>
            valor.ToString("0.############################", CultureInfo.InvariantCulture);

        private static string FormatearOpcional(decimal? valor) =>
            valor.HasValue ? FormatearDecimal(valor.Value) : string.Empty;

        private static byte[] SerializarUtf8(XDocument documento)
        {
            using var stream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
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
