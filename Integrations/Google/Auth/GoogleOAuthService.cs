using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Integrations.Google.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace Integrations.Google.Auth
{
    internal sealed class GoogleOAuthService : IGoogleOAuthService
    {
        private const string StateProtectorPurpose = "Lumo.Google.OAuth.State";

        private readonly IGoogleConfigRepository _googleConfigRepository;
        private readonly IGoogleAccountProvider _googleAccountProvider;
        private readonly IDataProtector _stateProtector;

        public GoogleOAuthService(IGoogleConfigRepository googleConfigRepository, IGoogleAccountProvider googleAccountProvider, IDataProtectionProvider dataProtectionProvider)
        {
            _googleConfigRepository = googleConfigRepository;
            _googleAccountProvider = googleAccountProvider;

            _stateProtector = dataProtectionProvider.CreateProtector(StateProtectorPurpose);
        }

        public async Task<string> CreateAuthorizationUrlAsync(int userId, CancellationToken cancellationToken = default)
        {
            var config = await _googleConfigRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Google: Brak aktywnej konfiguracji.");

            var flow = CreateFlow(config);

            var request = flow.CreateAuthorizationCodeRequest(config.RedirectUri);

            request.State = CreateState(userId);

            return request.Build().ToString();
        }

        public async Task HandleCallbackAsync(string code, string state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException(
                    "Google: Brak authorization code.");
            }

            var userId = ReadUserIdFromState(state);

            var config = await _googleConfigRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Google: Brak aktywnej konfiguracji.");

            var flow = CreateFlow(config);

            var token = await flow.ExchangeCodeForTokenAsync(userId.ToString(), code, config.RedirectUri, cancellationToken);

            if (string.IsNullOrWhiteSpace(token.IdToken))
            {
                throw new InvalidOperationException("Google nie zwrócił ID tokena.");
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [config.ClientId]
                });

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new InvalidOperationException("Google nie zwrócił adresu e-mail.");
            }

            await _googleAccountProvider.SaveAsync(new SaveGoogleAccountRequest(
                    UserId: userId,
                    Email: payload.Email,
                    RefreshToken: token.RefreshToken,
                    GrantedScopes: token.Scope), cancellationToken);
        }

        private string CreateState(int userId)
        {
            var state = new GoogleOAuthState(
                UserId: userId,
                CreatedAtUtc: DateTime.UtcNow);

            var json = JsonSerializer.Serialize(state);

            try
            {
                return _stateProtector.Protect(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== DATA PROTECTION ERROR ===");
                Console.WriteLine(ex.ToString());

                throw;
            }
        }

        private static GoogleAuthorizationCodeFlow CreateFlow(
        GoogleConfig config)
        {
            var scopes = config.Scopes.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = config.ClientId,
                        ClientSecret = config.ClientSecret
                    },

                    Scopes = scopes,

                    IncludeGrantedScopes = true,

                    Prompt = "consent"
                });
        }

        private int ReadUserIdFromState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException(
                    "Google: Brak parametru state.");
            }

            var json = _stateProtector.Unprotect(state);

            var oauthState = JsonSerializer.Deserialize<GoogleOAuthState>(json)
                ?? throw new InvalidOperationException("Google: Nieprawidłowy state.");

            if (oauthState.CreatedAtUtc < DateTime.UtcNow.AddMinutes(-10))
            {
                throw new InvalidOperationException("Google: State wygasł.");
            }

            return oauthState.UserId;
        }

        private sealed record GoogleOAuthState(
            int UserId,
            DateTime CreatedAtUtc);
    }
}
