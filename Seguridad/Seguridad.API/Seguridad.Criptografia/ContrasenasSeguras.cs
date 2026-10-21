using System.Security.Cryptography;
using System.Text;

namespace Seguridad.Criptografia
{
    public static class ContrasenasSeguras
    {
        private const int Iteraciones = 600_000;
        private const int SalBytes = 16;
        private const int HashBytes = 32;
        private const string Prefijo = "PBKDF2-SHA256";

        public static string Crear(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("La contraseña es obligatoria.", nameof(contrasena));
            byte[] sal = RandomNumberGenerator.GetBytes(SalBytes);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones,
                HashAlgorithmName.SHA256, HashBytes);
            return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string contrasena, string almacenado, out bool requiereMigracion)
        {
            requiereMigracion = false;
            if (string.IsNullOrEmpty(contrasena) || string.IsNullOrEmpty(almacenado)) return false;
            var partes = almacenado.Split('$');
            if (partes.Length == 4 && partes[0] == Prefijo)
            {
                if (!int.TryParse(partes[1], out int iteraciones) || iteraciones < 100_000 || iteraciones > 2_000_000)
                    return false;
                try
                {
                    byte[] sal = Convert.FromBase64String(partes[2]);
                    byte[] esperado = Convert.FromBase64String(partes[3]);
                    if (sal.Length != SalBytes || esperado.Length != HashBytes) return false;
                    byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones,
                        HashAlgorithmName.SHA256, esperado.Length);
                    return CryptographicOperations.FixedTimeEquals(calculado, esperado);
                }
                catch (FormatException) { return false; }
            }
            if (almacenado.Length != 64 || !almacenado.All(Uri.IsHexDigit)) return false;
            byte[] anterior = SHA256.HashData(Encoding.UTF8.GetBytes(contrasena));
            byte[] esperadoAnterior = Convert.FromHexString(almacenado);
            bool coincide = CryptographicOperations.FixedTimeEquals(anterior, esperadoAnterior);
            requiereMigracion = coincide;
            return coincide;
        }
    }
}
