using System.Security.Cryptography;
using System.Text;
namespace Seguridad.Criptografia
{
    public static class CifradoEnlaces
    {
        public static string Cifrar(string token,byte[] clave)
        {
            var plano=Encoding.UTF8.GetBytes(token);var nonce=RandomNumberGenerator.GetBytes(12);
            var cifrado=new byte[plano.Length];var etiqueta=new byte[16];
            try
            {
                using var aes=new AesGcm(clave,16);
                aes.Encrypt(nonce,plano,cifrado,etiqueta,SHA256.HashData(plano));
                return "v1."+Convert.ToBase64String(nonce.Concat(etiqueta).Concat(cifrado).ToArray());
            }
            finally{CryptographicOperations.ZeroMemory(plano);}
        }
        public static string Descifrar(string valor,byte[] clave,byte[] hashToken)
        {
            if(!valor.StartsWith("v1.",StringComparison.Ordinal) || valor.Length>500 || hashToken.Length!=32)
                throw new CryptographicException("Contenido de enlace invalido.");
            var datos=Convert.FromBase64String(valor[3..]);
            if(datos.Length!=92)throw new CryptographicException("Contenido de enlace invalido.");
            var plano=new byte[64];
            try
            {
                using var aes=new AesGcm(clave,16);
                aes.Decrypt(datos.AsSpan(0,12),datos.AsSpan(28),datos.AsSpan(12,16),plano,hashToken);
                if(!CryptographicOperations.FixedTimeEquals(SHA256.HashData(plano),hashToken))throw new CryptographicException();
                return Encoding.UTF8.GetString(plano);
            }
            finally{CryptographicOperations.ZeroMemory(plano);}
        }
    }
}