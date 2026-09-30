using Integrations.TikTok.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Integrations.TikTok.Auth
{
    internal sealed class TikTokCredentialProvider : ITikTokCredentialProvider
    {
        private readonly ITikTokAccountProvider _accountProvider;
        private readonly ITikTokConfigRepository _configRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public TikTokCredentialProvider(
            ITikTokAccountProvider accountProvider,
            ITikTokConfigRepository configRepository,
            IHttpClientFactory httpClientFactory)
        {
            _accountProvider = accountProvider;
            _configRepository = configRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAccessTokenAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var account =
                await _accountProvider.GetAsync(
                    userId,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "TikTok: Użytkownik nie ma połączonego konta.");

            if (account.AccessTokenExpiresAt >
                DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return account.AccessTokenProtected;
            }

            return await RefreshAsync(
                userId,
                account,
                cancellationToken);
        }

        private async Task<string> RefreshAsync(
            int userId,
            TikTokAccount account,
            CancellationToken cancellationToken)
        {
            if (account.RefreshTokenExpiresAt <=
                DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException(
                    "TikTok: Refresh token wygasł. Wymagana ponowna autoryzacja.");
            }

            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "TikTok: Brak aktywnej konfiguracji.");

            var client =
                _httpClientFactory.CreateClient("tiktok");

            var request = new Dictionary<string, string>
            {
                ["client_key"] = config.ClientKey,
                ["client_secret"] = config.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = account.RefreshTokenProtected
            };

            var response = await client.PostAsync(
                "/v2/oauth/token/",
                new FormUrlEncodedContent(request),
                cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"TikTok refresh token error " +
                    $"{(int)response.StatusCode}: {json}");
            }

            var tokenResponse =
                JsonSerializer.Deserialize<TikTokOAuthResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (tokenResponse is null ||
                string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    $"TikTok nie zwrócił access tokena podczas refresh: {json}");
            }

            await _accountProvider.SaveAsync(
                new SaveTikTokAccountRequest(
                    UserId: userId,
                    OpenId: account.OpenId,
                    AccessToken: tokenResponse.AccessToken,
                    RefreshToken:
                        string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)
                            ? account.RefreshTokenProtected
                            : tokenResponse.RefreshToken,
                    GrantedScopes: tokenResponse.Scope,
                    AccessTokenExpiresAt:
                        DateTimeOffset.UtcNow.AddSeconds(
                            tokenResponse.ExpiresIn),
                    RefreshTokenExpiresAt:
                        DateTimeOffset.UtcNow.AddSeconds(
                            tokenResponse.RefreshExpiresIn)),
                cancellationToken);

            return tokenResponse.AccessToken;
        }
    }
}
