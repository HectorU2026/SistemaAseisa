namespace Abstracciones.Modelos
{
    // Solo uso interno del despachador. No devolver por HTTP ni registrar sus propiedades.
    public sealed class CorreoEnlacePendiente:DestinatarioContrasena
    {
        public bool Procesable{get;set;}
        public long IdCorreo{get;set;}
        public Guid Reserva{get;set;}
        public string TokenCifrado{get;set;}="";
        public byte[] HashToken{get;set;}=Array.Empty<byte>();
    }
}