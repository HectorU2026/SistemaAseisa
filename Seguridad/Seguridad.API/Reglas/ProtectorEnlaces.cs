using Abstracciones.Reglas;
using Microsoft.Extensions.Configuration;
using Seguridad.Criptografia;
using System.Security.Cryptography;
namespace Reglas
{
    public sealed class ProtectorEnlaces:IProtectorEnlace
    {
        private readonly IConfiguration _configuracion;
        public ProtectorEnlaces(IConfiguration configuracion){_configuracion=configuracion;}
        private byte[] Clave()
        {
            try
            {
                var clave=Convert.FromBase64String(_configuracion["CorreoEnlaces:ClaveCifrado"]??"");
                if(clave.Length==32)return clave;
                CryptographicOperations.ZeroMemory(clave);
            }
            catch(FormatException){}
            throw new InvalidOperationException("Configurar la clave de cifrado de enlaces.");
        }
        public void ValidarConfiguracion(){var clave=Clave();CryptographicOperations.ZeroMemory(clave);}
        public string Cifrar(string token){var clave=Clave();try{return CifradoEnlaces.Cifrar(token,clave);}finally{CryptographicOperations.ZeroMemory(clave);}}
        public string Descifrar(string cifrado,byte[] hashToken){var clave=Clave();try{return CifradoEnlaces.Descifrar(cifrado,clave,hashToken);}finally{CryptographicOperations.ZeroMemory(clave);}}
    }
}