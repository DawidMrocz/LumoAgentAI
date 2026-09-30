using Integrations.Google.Auth.Models;

namespace Integrations.Google.Auth
{
    public interface IGoogleSignInService
    {
        /// <summary>
        /// Waliduje ID token wystawiony przez Google Sign-In (np. z przycisku logowania na froncie)
        /// i zwraca zweryfikowane dane tożsamości użytkownika.
        /// </summary>
        /// <exception cref="InvalidOperationException">Brak aktywnej konfiguracji Google.</exception>
        /// <exception cref="UnauthorizedAccessException">Token nieprawidłowy, wystawiony przez inny issuer lub e-mail niezweryfikowany.</exception>
        Task<GoogleSignInPayload> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
    }
}
