using System.Security.Cryptography;
using System.Text;

namespace Lumo.Api.Helpers
{
    /// <summary>
    /// Generowanie i hashowanie jednorazowych tokenów wysyłanych mailem
    /// (aktywacja konta, reset hasła).
    /// W bazie trzymamy WYŁĄCZNIE hash - wyciek bazy nie może pozwolić
    /// na przejęcie konta przez reset hasła.
    /// </summary>
    public static class SecureTokenGenerator
    {
        /// <summary>
        /// Token w formie URL-safe (base64url), gotowy do wstawienia w link bez dodatkowego kodowania.
        /// </summary>
        public static string Generate()
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        }

        public static string Hash(string token)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Porównanie w czasie stałym - chroni przed atakami czasowymi na porównywanie tokenów.
        /// </summary>
        public static bool Verify(string token, string expectedHash)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expectedHash))
            {
                return false;
            }

            var actual = Encoding.UTF8.GetBytes(Hash(token));
            var expected = Encoding.UTF8.GetBytes(expectedHash);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
