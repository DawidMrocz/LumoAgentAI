using Integrations.Meta.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using System.Text.Json;

namespace Integrations.Meta.Auth
{
    internal sealed class MetaSignInService : IMetaSignInService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMetaConfigRepository _configRepository;

        public MetaSignInService(
            IHttpClientFactory httpClientFactory,
            IMetaConfigRepository configRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configRepository = configRepository;
        }

        public async Task<MetaSignInPayload> ValidateAccessTokenAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Meta: Access token jest wymagany.", nameof(accessToken));
            }

            var config = await _configRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Meta: Brak aktywnej konfiguracji.");

            await EnsureTokenBelongsToAppAsync(accessToken, config, cancellationToken);

            var (id, email, firstName, lastName) = await GetMeAsync(accessToken, config, cancellationToken);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new UnauthorizedAccessException("Meta: Nie udało się pobrać identyfikatora użytkownika.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedAccessException("Meta: Konto Facebook nie ma zweryfikowanego adresu e-mail.");
            }

            return new MetaSignInPayload(
                Subject: id,
                Email: email,
                FirstName: firstName,
                LastName: lastName);
        }

        private async Task EnsureTokenBelongsToAppAsync(
            string accessToken,
            MetaConfig config,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("meta");

            var appAccessToken = $"{config.AppId}|{config.AppSecret}";

            var url =
                $"/{config.ApiVersion}/debug_token" +
                $"?input_token={Uri.EscapeDataString(accessToken)}" +
                $"&access_token={Uri.EscapeDataString(appAccessToken)}";

            var response = await client.GetAsync(url, cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException($"Meta: Nie udało się zweryfikować tokena ({(int)response.StatusCode}): {json}");
            }

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("data", out var data))
            {
                throw new UnauthorizedAccessException("Meta: Nieprawidłowa odpowiedź debug_token.");
            }

            var isValid = data.TryGetProperty("is_valid", out var isValidElement) && isValidElement.GetBoolean();

            if (!isValid)
            {
                throw new UnauthorizedAccessException("Meta: Access token jest nieprawidłowy lub wygasł.");
            }

            var appId = data.TryGetProperty("app_id", out var appIdElement) ? appIdElement.GetString() : null;

            if (!string.Equals(appId, config.AppId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Meta: Access token został wystawiony dla innej aplikacji.");
            }
        }

        private async Task<(string? Id, string? Email, string? FirstName, string? LastName)> GetMeAsync(
            string accessToken,
            MetaConfig config,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("meta");

            var proof = MetaSignatureHelper.GenerateAppSecretProof(accessToken, config.AppSecret);

            var url =
                $"/{config.ApiVersion}/me" +
                $"?access_token={Uri.EscapeDataString(accessToken)}" +
                $"&appsecret_proof={Uri.EscapeDataString(proof)}" +
                "&fields=id,email,first_name,last_name";

            var response = await client.GetAsync(url, cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException($"Meta Graph API error {(int)response.StatusCode}: {json}");
            }

            using var document = JsonDocument.Parse(json);

            var root = document.RootElement;

            var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var email = root.TryGetProperty("email", out var emailElement) ? emailElement.GetString() : null;
            var firstName = root.TryGetProperty("first_name", out var firstNameElement) ? firstNameElement.GetString() : null;
            var lastName = root.TryGetProperty("last_name", out var lastNameElement) ? lastNameElement.GetString() : null;

            return (id, email, firstName, lastName);
        }
    }
}
