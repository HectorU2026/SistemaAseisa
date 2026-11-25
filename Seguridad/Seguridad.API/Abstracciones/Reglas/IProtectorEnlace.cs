namespace Abstracciones.Reglas
{
    public interface IProtectorEnlace
    {
        void ValidarConfiguracion();
        string Cifrar(string token);
        string Descifrar(string cifrado,byte[] hashToken);
    }
}