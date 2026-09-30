using Integrations.TikTok.Auth.Models;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Integrations.TikTok.Auth
{
    internal sealed class TikTokOAuthService : ITikTokOAuthService
    {
        private const string StateProtectorPurpose =
            "Lumo.TikTok.OAuth.State";

        private readonly ITikTokConfigRepository _configRepository;
        private readonly ITikTokAccountProvider _accountProvider;
        private readonly IDataProtector _stateProtector;
        private readonly IHttpClientFactory _httpClientFactory;

        public TikTokOAuthService(
            ITikTokConfigRepository configRepository,
            ITikTokAccountProvider accountProvider,
            IDataProtectionProvider dataProtectionProvider,
            IHttpClientFactory httpClientFactory)
        {
            _configRepository = configRepository;
            _accountProvider = accountProvider;
            _httpClientFactory = httpClientFactory;

            _stateProtector =
                dataProtectionProvider.CreateProtector(
                    StateProtectorPurpose);
        }

        public async Task<string> CreateAuthorizationUrlAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "TikTok: Brak aktywnej konfiguracji.");

            var state = CreateState(userId);

            var scopes =
                string.Join(",", config.Scopes);

            var query = new Dictionary<string, string>
            {
                ["client_key"] = config.ClientKey,
                ["scope"] = scopes,
                ["response_type"] = "code",
                ["redirect_uri"] = config.RedirectUri,
                ["state"] = state
            };

            var queryString = string.Join(
                "&",
                query.Select(x =>
                    $"{Uri.EscapeDataString(x.Key)}=" +
                    $"{Uri.EscapeDataString(x.Value)}"));

            return
                $"{config.AuthBaseUrl}/v2/auth/authorize/?" +
                queryString;
        }

        public async Task HandleCallbackAsync(
            string code,
            string state,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException(
                    "TikTok: Brak authorization code.");

            var userId = ReadUserIdFromState(state);

            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "TikTok: Brak aktywnej konfiguracji.");

            var client =
                _httpClientFactory.CreateClient("tiktok");

            var form = new Dictionary<string, string>
            {
                ["client_key"] = config.ClientKey,
                ["client_secret"] = config.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = config.RedirectUri
            };

            var response = await client.PostAsync(
                "/v2/oauth/token/",
                new FormUrlEncodedContent(form),
                cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"TikTok OAuth error " +
                    $"{(int)response.StatusCode}: {json}");
            }

            var tokenResponse =
                JsonSerializer.Deserialize<TikTokOAuthResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (tokenResponse is null)
                throw new InvalidOperationException(
                    "TikTok: Nieprawidłowa odpowiedź OAuth.");

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException(
                    "TikTok: Brak access tokena.");

            if (string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
                throw new InvalidOperationException(
                    "TikTok: Brak refresh tokena.");

            await _accountProvider.SaveAsync(
                new SaveTikTokAccountRequest(
                    UserId: userId,
                    OpenId: tokenResponse.OpenId,
                    AccessToken: tokenResponse.AccessToken,
                    RefreshToken: tokenResponse.RefreshToken,
                    GrantedScopes: tokenResponse.Scope,
                    AccessTokenExpiresAt:
                        DateTimeOffset.UtcNow.AddSeconds(
                            tokenResponse.ExpiresIn),
                    RefreshTokenExpiresAt:
                        DateTimeOffset.UtcNow.AddSeconds(
                            tokenResponse.RefreshExpiresIn)),
                cancellationToken);
        }

        private string CreateState(int userId)
        {
            var state = new TikTokOAuthState(
                userId,
                DateTimeOffset.UtcNow);

            var json =
                JsonSerializer.Serialize(state);

            return _stateProtector.Protect(json);
        }

        private int ReadUserIdFromState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                throw new InvalidOperationException(
                    "TikTok: Brak state.");

            var json =
                _stateProtector.Unprotect(state);

            var oauthState =
                JsonSerializer.Deserialize<TikTokOAuthState>(
                    json);

            if (oauthState is null)
                throw new InvalidOperationException(
                    "TikTok: Nieprawidłowy state.");

            if (oauthState.CreatedAtUtc <
                DateTimeOffset.UtcNow.AddMinutes(-10))
            {
                throw new InvalidOperationException(
                    "TikTok: State wygasł.");
            }

            return oauthState.UserId;
        }

        private sealed record TikTokOAuthState(
            int UserId,
            DateTimeOffset CreatedAtUtc);
    }
}
