using Integrations.Meta.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace Integrations.Meta.Auth
{
    internal sealed class MetaOAuthService : IMetaOAuthService
    {
        private const string StateProtectorPurpose =
            "Lumo.Meta.OAuth.State";

        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

        private readonly IMetaConfigRepository _configRepository;

        private readonly IMetaAccountProvider _accountProvider;

        private readonly IDataProtector _stateProtector;

        public MetaOAuthService(
            System.Net.Http.IHttpClientFactory httpClientFactory,
            IMetaConfigRepository configRepository,
            IMetaAccountProvider accountProvider,
            IDataProtectionProvider dataProtectionProvider)
        {
            _httpClientFactory = httpClientFactory;
            _configRepository = configRepository;
            _accountProvider = accountProvider;

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
                    "Meta: Brak aktywnej konfiguracji.");

            var state = CreateState(userId);

            var scopes = config.Scopes
                .Split(
                    [' ', ',', ';', '\r', '\n', '\t'],
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            var url =
                $"https://www.facebook.com/{config.ApiVersion}/dialog/oauth" +
                $"?client_id={Uri.EscapeDataString(config.AppId)}" +
                $"&redirect_uri={Uri.EscapeDataString(config.RedirectUri)}" +
                $"&config_id={Uri.EscapeDataString(config.ConfigId)}" +
                $"&response_type=code" +
                $"&state={Uri.EscapeDataString(state)}";

            if (scopes.Length > 0)
            {
                url +=
                    $"&scope={Uri.EscapeDataString(
                        string.Join(",", scopes))}";
            }

            return url;
        }

        public async Task HandleCallbackAsync(
            string code,
            string state,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException(
                    "Meta: Brak authorization code.");
            }

            var userId = ReadUserIdFromState(state);

            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Meta: Brak aktywnej konfiguracji.");

            var shortLivedToken =
                await ExchangeCodeForAccessTokenAsync(
                    code,
                    config,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(shortLivedToken))
            {
                throw new InvalidOperationException(
                    "Meta nie zwróciła access tokena.");
            }

            var longLivedToken =
                await ExchangeShortLivedForLongLivedAsync(
                    shortLivedToken,
                    config,
                    cancellationToken);

            var me =
                await GetMeAsync(
                    longLivedToken,
                    config,
                    cancellationToken);

            await _accountProvider.SaveAsync(
                new SaveMetaAccountRequest(
                    UserId: userId,
                    MetaUserId: me.Id,
                    Email: me.Email,
                    AccessToken: longLivedToken,
                    GrantedScopes: config.Scopes,
                    TokenExpiresAtUtc: me.ExpiresAtUtc),
                cancellationToken);
        }

        private async Task<string> ExchangeCodeForAccessTokenAsync(
            string code,
            MetaConfig config,
            CancellationToken cancellationToken)
        {
            var client =
                _httpClientFactory.CreateClient("meta");

            var url =
                $"/{config.ApiVersion}/oauth/access_token" +
                $"?client_id={Uri.EscapeDataString(config.AppId)}" +
                $"&redirect_uri={Uri.EscapeDataString(config.RedirectUri)}" +
                $"&client_secret={Uri.EscapeDataString(config.AppSecret)}" +
                $"&code={Uri.EscapeDataString(code)}";

            var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Meta OAuth error {(int)response.StatusCode}: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty(
                    "access_token",
                    out var tokenElement))
            {
                return tokenElement.GetString()
                    ?? throw new InvalidOperationException(
                        "Meta zwróciła pusty access token.");
            }

            throw new InvalidOperationException(
                $"Brak access_token w odpowiedzi Meta: {json}");
        }

        private async Task<string>
            ExchangeShortLivedForLongLivedAsync(
                string shortLivedToken,
                MetaConfig config,
                CancellationToken cancellationToken)
        {
            var client =
                _httpClientFactory.CreateClient("meta");

            var url =
                $"/{config.ApiVersion}/oauth/access_token" +
                $"?grant_type=fb_exchange_token" +
                $"&client_id={Uri.EscapeDataString(config.AppId)}" +
                $"&client_secret={Uri.EscapeDataString(config.AppSecret)}" +
                $"&fb_exchange_token={Uri.EscapeDataString(shortLivedToken)}";

            var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Meta OAuth error {(int)response.StatusCode}: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty(
                    "access_token",
                    out var tokenElement))
            {
                return tokenElement.GetString()
                    ?? throw new InvalidOperationException(
                        "Meta zwróciła pusty long-lived access token.");
            }

            throw new InvalidOperationException(
                $"Brak access_token w odpowiedzi Meta: {json}");
        }

        private async Task<MetaUserInfo> GetMeAsync(
            string accessToken,
            MetaConfig config,
            CancellationToken cancellationToken)
        {
            var client =
                _httpClientFactory.CreateClient("meta");

            var proof =
                MetaSignatureHelper.GenerateAppSecretProof(
                    accessToken,
                    config.AppSecret);

            var url =
                $"/{config.ApiVersion}/me" +
                $"?access_token={Uri.EscapeDataString(accessToken)}" +
                $"&appsecret_proof={Uri.EscapeDataString(proof)}" +
                "&fields=id,name,email";

            var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Meta Graph API error {(int)response.StatusCode}: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            var root = document.RootElement;

            var id =
                root.TryGetProperty("id", out var idElement)
                    ? idElement.GetString()
                    : null;

            var email =
                root.TryGetProperty("email", out var emailElement)
                    ? emailElement.GetString()
                    : null;

            return new MetaUserInfo(
                id,
                email,
                null);
        }

        private string CreateState(int userId)
        {
            var state = new MetaOAuthState(
                UserId: userId,
                CreatedAtUtc: DateTime.UtcNow);

            var json =
                JsonSerializer.Serialize(state);

            return _stateProtector.Protect(json);
        }

        private int ReadUserIdFromState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException(
                    "Meta: Brak parametru state.");
            }

            var json =
                _stateProtector.Unprotect(state);

            var oauthState =
                JsonSerializer.Deserialize<MetaOAuthState>(json)
                ?? throw new InvalidOperationException(
                    "Meta: Nieprawidłowy state.");

            if (oauthState.CreatedAtUtc <
                DateTime.UtcNow.AddMinutes(-10))
            {
                throw new InvalidOperationException(
                    "Meta: State wygasł.");
            }

            return oauthState.UserId;
        }

        private sealed record MetaOAuthState(
            int UserId,
            DateTime CreatedAtUtc);

        private sealed record MetaUserInfo(
            string? Id,
            string? Email,
            DateTime? ExpiresAtUtc);
    }
}
