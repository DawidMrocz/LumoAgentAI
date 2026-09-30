using Integrations.Meta.Auth.Models;

namespace Integrations.Meta.Auth
{
    public interface IMetaSignInService
    {
        /// <summary>
        /// Waliduje access token wystawiony przez Facebook Login (np. z Facebook JS SDK na froncie)
        /// i zwraca zweryfikowane dane tożsamości użytkownika.
        /// </summary>
        /// <exception cref="InvalidOperationException">Brak aktywnej konfiguracji Meta.</exception>
        /// <exception cref="UnauthorizedAccessException">Token nieprawidłowy, wystawiony dla innej aplikacji lub brak zweryfikowanego e-maila.</exception>
        Task<MetaSignInPayload> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}
