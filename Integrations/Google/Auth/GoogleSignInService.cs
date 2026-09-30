using Google.Apis.Auth;
using Integrations.Google.Auth.Models;
using Lumo.Infrastructure.Repositories;

namespace Integrations.Google.Auth
{
    internal sealed class GoogleSignInService : IGoogleSignInService
    {
        private static readonly string[] ValidIssuers = ["accounts.google.com", "https://accounts.google.com"];

        private readonly IGoogleConfigRepository _googleConfigRepository;

        public GoogleSignInService(IGoogleConfigRepository googleConfigRepository)
        {
            _googleConfigRepository = googleConfigRepository;
        }

        public async Task<GoogleSignInPayload> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new ArgumentException("Google: ID token jest wymagany.", nameof(idToken));
            }

            var config = await _googleConfigRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Google: Brak aktywnej konfiguracji.");

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [config.ClientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (!ValidIssuers.Contains(payload.Issuer))
            {
                throw new UnauthorizedAccessException("Google: Nieprawidłowy issuer tokena.");
            }

            if (!payload.EmailVerified)
            {
                throw new UnauthorizedAccessException("Google: Adres e-mail nie został zweryfikowany.");
            }

            return new GoogleSignInPayload(
                Subject: payload.Subject,
                Email: payload.Email,
                FirstName: payload.GivenName,
                LastName: payload.FamilyName);
        }
    }
}
