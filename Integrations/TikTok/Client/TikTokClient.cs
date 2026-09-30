using Integrations.TikTok.Auth;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Integrations.TikTok.Client
{
    internal sealed class TikTokClient : ITikTokClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITikTokCredentialProvider _credentialProvider;

        public TikTokClient(
            IHttpClientFactory httpClientFactory,
            ITikTokCredentialProvider credentialProvider)
        {
            _httpClientFactory = httpClientFactory;
            _credentialProvider = credentialProvider;
        }

        public async Task<T?> GetAsync<T>(
            string path,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateClientAsync(
                userId,
                cancellationToken);

            var response =
                await client.GetAsync(
                    path,
                    cancellationToken);

            await EnsureSuccessAsync(response);

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

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
            object body,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateClientAsync(
                userId,
                cancellationToken);

            var json =
                JsonSerializer.Serialize(body);

            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            var response =
                await client.PostAsync(
                    path,
                    content,
                    cancellationToken);

            await EnsureSuccessAsync(response);

            var responseJson =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            return JsonSerializer.Deserialize<T>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<HttpResponseMessage> PostRawAsync(
            string path,
            int userId,
            HttpContent content,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateClientAsync(
                userId,
                cancellationToken);

            var response =
                await client.PostAsync(
                    path,
                    content,
                    cancellationToken);

            await EnsureSuccessAsync(response);

            return response;
        }

        public async Task<HttpResponseMessage> PutRawAsync(
            string path,
            int userId,
            HttpContent content,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateClientAsync(
                userId,
                cancellationToken);

            var response =
                await client.PutAsync(
                    path,
                    content,
                    cancellationToken);

            await EnsureSuccessAsync(response);

            return response;
        }

        private async Task<HttpClient> CreateClientAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var token =
                await _credentialProvider.GetAccessTokenAsync(
                    userId,
                    cancellationToken);

            var client =
                _httpClientFactory.CreateClient("tiktok");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"TikTok API error " +
                $"{(int)response.StatusCode}: {body}");
        }
    }
}
