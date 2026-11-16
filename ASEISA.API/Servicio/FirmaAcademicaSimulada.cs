using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Abstracciones.Interfaces.Servicio;

namespace Servicio
{
    public class FirmaAcademicaSimulada : IFirmaAcademicaSimulada
    {
        public const string AlgoritmoHuella = "SHA-256-academico-simulado";
        public const string TipoFirma = "simulada-no-tributaria";
        public const string AvisoSimulacion =
            "Firma académica simulada. No es XMLDSIG, no usa certificado X.509 y no tiene validez ante el Ministerio de Hacienda ni ante la DGT.";

        private static readonly XNamespace NamespaceAcademico = GeneradorXmlFactura44Academico.NamespaceAcademico;

        public string Algoritmo => AlgoritmoHuella;

        public FirmaAcademicaSimuladaResultado Firmar(byte[] xmlSinFirma)
        {
            ArgumentNullException.ThrowIfNull(xmlSinFirma);
            if (xmlSinFirma.Length == 0)
                throw new InvalidOperationException("El XML académico a firmar es inválido.");

            var documento = Cargar(xmlSinFirma);
            var raiz = documento.Root
                ?? throw new InvalidOperationException("El XML académico a firmar es inválido.");

            QuitarFirmaSimulada(raiz);

            var contenido = SerializarUtf8(documento);
            var huella = CalcularHuella(contenido);

            raiz.Add(CrearNodoFirma(huella));

            return new FirmaAcademicaSimuladaResultado
            {
                Xml = SerializarUtf8(documento),
                HuellaContenido = huella,
                Algoritmo = AlgoritmoHuella
            };
        }

        public bool Verificar(byte[] xmlFirmado)
        {
            if (xmlFirmado == null || xmlFirmado.Length == 0)
                return false;

            try
            {
                var documento = Cargar(xmlFirmado);
                var raiz = documento.Root;
                if (raiz == null)
                    return false;

                var nodoFirma = ObtenerNodoFirmaUnico(raiz);
                if (nodoFirma == null)
                    return false;

                var huellaDeclarada = ((string?)nodoFirma.Element(NamespaceAcademico + "HuellaContenido"))
                    ?.Trim()
                    .ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(huellaDeclarada))
                    return false;

                nodoFirma.Remove();
                var huellaCalculada = CalcularHuella(SerializarUtf8(documento));
                var declaradaBytes = Encoding.ASCII.GetBytes(huellaDeclarada);
                var calculadaBytes = Encoding.ASCII.GetBytes(huellaCalculada);
                if (declaradaBytes.Length != calculadaBytes.Length)
                    return false;

                return CryptographicOperations.FixedTimeEquals(calculadaBytes, declaradaBytes);
            }
            catch (XmlException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static XElement CrearNodoFirma(string huella)
        {
            return new XElement(NamespaceAcademico + "FirmaAcademicaSimulada",
                new XElement(NamespaceAcademico + "Tipo", TipoFirma),
                new XElement(NamespaceAcademico + "Algoritmo", AlgoritmoHuella),
                new XElement(NamespaceAcademico + "HuellaContenido", huella),
                new XElement(NamespaceAcademico + "Aviso", AvisoSimulacion));
        }

        private static void QuitarFirmaSimulada(XElement raiz)
        {
            foreach (var nodo in raiz.Elements(NamespaceAcademico + "FirmaAcademicaSimulada").ToList())
                nodo.Remove();
        }

        private static XElement? ObtenerNodoFirmaUnico(XElement raiz)
        {
            var nodos = raiz.Elements(NamespaceAcademico + "FirmaAcademicaSimulada").ToList();
            return nodos.Count == 1 ? nodos[0] : null;
        }

        private static string CalcularHuella(byte[] contenido) =>
            Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant();

        private static XDocument Cargar(byte[] xml)
        {
            using var stream = new MemoryStream(xml);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreWhitespace = false
            });
            return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        }

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
