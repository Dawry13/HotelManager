using System;
using System.Linq;
using System.Security.Cryptography;

namespace HotelManager.Business.Seguridad
{
    /// <summary>
    /// Hash de contrasenas con PBKDF2-SHA256 (sal aleatoria por usuario y 100.000 iteraciones).
    /// Formato guardado: PBKDF2-SHA256$iteraciones$sal(base64)$hash(base64). Nunca se guarda la clave.
    /// </summary>
    public static class ClaveSegura
    {
        private const string Algoritmo = "PBKDF2-SHA256";
        private const int Iteraciones = 100000;
        private const int BytesSal = 16;
        private const int BytesHash = 32;

        public static string Hash(string clave)
        {
            if (clave == null) throw new ArgumentNullException(nameof(clave));

            var sal = new byte[BytesSal];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(sal);

            var hash = Derivar(clave, sal, Iteraciones);
            return Algoritmo + "$" + Iteraciones + "$" + Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string clave, string guardado)
        {
            if (clave == null || string.IsNullOrEmpty(guardado)) return false;

            var partes = guardado.Split('$');
            int iteraciones;
            if (partes.Length != 4 || partes[0] != Algoritmo || !int.TryParse(partes[1], out iteraciones)) return false;

            try
            {
                var sal = Convert.FromBase64String(partes[2]);
                var esperado = Convert.FromBase64String(partes[3]);
                var calculado = Derivar(clave, sal, iteraciones);
                return IgualesTiempoConstante(esperado, calculado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>Politica minima: 8 caracteres con al menos una letra y un numero.</summary>
        public static string ValidarPolitica(string clave)
        {
            if (string.IsNullOrEmpty(clave) || clave.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres.";
            if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
                return "La contraseña debe combinar letras y números.";
            return null;
        }

        private static byte[] Derivar(string clave, byte[] sal, int iteraciones)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(clave, sal, iteraciones, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(BytesHash);
        }

        // Compara sin cortar en el primer byte distinto (evita ataques de tiempo).
        private static bool IgualesTiempoConstante(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diferencia = 0;
            for (var i = 0; i < a.Length; i++) diferencia |= a[i] ^ b[i];
            return diferencia == 0;
        }
    }
}
