using System.Security.Cryptography;
using System.Text;

namespace Integrations.Meta.Auth
{
    internal static class MetaSignatureHelper
    {
        /// <summary>
        /// Generuje appsecret_proof wymagany przez Graph API przy wywołaniach z access tokenem,
        /// gdy w ustawieniach aplikacji Meta włączone jest "Require App Secret".
        /// </summary>
        public static string GenerateAppSecretProof(string accessToken, string appSecret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(accessToken));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
