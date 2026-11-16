namespace Abstracciones.Interfaces.Servicio
{
    public interface IFirmaAcademicaSimulada
    {
        string Algoritmo { get; }

        FirmaAcademicaSimuladaResultado Firmar(byte[] xmlSinFirma);

        bool Verificar(byte[] xmlFirmado);
    }

    public sealed class FirmaAcademicaSimuladaResultado
    {
        public required byte[] Xml { get; init; }

        public required string HuellaContenido { get; init; }

        public required string Algoritmo { get; init; }
    }
}
