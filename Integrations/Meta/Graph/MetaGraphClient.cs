using Integrations.Meta.Auth;
using Lumo.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integrations.Meta.Graph
{
    internal sealed class MetaGraphClient : IMetaGraphClient
    {
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly IMetaCredentialProvider _credentialProvider;
        private readonly IMetaConfigRepository _configRepository;

        public MetaGraphClient(
            System.Net.Http.IHttpClientFactory httpClientFactory,
            IMetaCredentialProvider credentialProvider,
            IMetaConfigRepository configRepository)
        {
            _httpClientFactory = httpClientFactory;
            _credentialProvider = credentialProvider;
            _configRepository = configRepository;
        }

        public async Task<T?> GetAsync<T>(
            string path,
            int userId,
            Dictionary<string, string>? query = null,
            CancellationToken cancellationToken = default)
        {
            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Meta: Brak aktywnej konfiguracji.");

            var accessToken =
                await _credentialProvider.GetAccessTokenAsync(
                    userId,
                    cancellationToken);

            var appSecretProof =
                GenerateAppSecretProof(
                    accessToken,
                    config.AppSecret);

            var values =
                new List<string>
                {
                $"access_token={Uri.EscapeDataString(accessToken)}",
                $"appsecret_proof={Uri.EscapeDataString(appSecretProof)}"
                };

            if (query is not null)
            {
                values.AddRange(
                    query.Select(x =>
                        $"{Uri.EscapeDataString(x.Key)}=" +
                        $"{Uri.EscapeDataString(x.Value)}"));
            }

            var url =
                $"/{config.ApiVersion}/{path}" +
                $"?{string.Join("&", values)}";

            var client =
                _httpClientFactory.CreateClient("meta");

            var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            EnsureSuccess(
                response,
                json);

            if (typeof(T) == typeof(string))
            {
                return (T)(object)json;
            }

            return JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<T?> PostAsync<T>(
            string path,
            int userId,
            Dictionary<string, string> formValues,
            CancellationToken cancellationToken = default)
        {
            var config =
                await _configRepository.GetActiveAsync(
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Meta: Brak aktywnej konfiguracji.");

            var accessToken =
                await _credentialProvider.GetAccessTokenAsync(
                    userId,
                    cancellationToken);

            var values =
                new Dictionary<string, string>(
                    formValues)
                {
                    ["access_token"] = accessToken,

                    ["appsecret_proof"] =
                        GenerateAppSecretProof(
                            accessToken,
                            config.AppSecret)
                };

            var client =
                _httpClientFactory.CreateClient("meta");

            var response =
                await client.PostAsync(
                    $"/{config.ApiVersion}/{path}",
                    new FormUrlEncodedContent(values),
                    cancellationToken);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            EnsureSuccess(
                response,
                json);

            if (typeof(T) == typeof(string))
            {
                return (T)(object)json;
            }

            return JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private static void EnsureSuccess(
            HttpResponseMessage response,
            string json)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Meta Graph API error " +
                    $"{(int)response.StatusCode}: {json}");
            }
        }

        private static string GenerateAppSecretProof(
            string accessToken,
            string appSecret)
        {
            using var hmac =
                new HMACSHA256(
                    Encoding.UTF8.GetBytes(appSecret));

            var hash =
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(accessToken));

            return Convert.ToHexString(hash)
                .ToLowerInvariant();
        }
    }
}
