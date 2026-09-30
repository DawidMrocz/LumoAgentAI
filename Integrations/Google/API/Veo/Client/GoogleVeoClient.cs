using Integrations.Google.API.Veo.Client.Models;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Integrations.Google.API.Veo.Client;

internal sealed class GoogleVeoClient : IGoogleVeoClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IVeoConfigRepository _veoConfigRepository;
    private readonly ILogger<GoogleVeoClient> _logger;

    public GoogleVeoClient(
        IHttpClientFactory httpClientFactory,
        IVeoConfigRepository veoConfigRepository,
        ILogger<GoogleVeoClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _veoConfigRepository = veoConfigRepository;
        _logger = logger;
    }

    public async Task<string?> PredictAsync(
        VeoApiRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetActiveConfigAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();

            var endpoint = $"{config.ApiBaseUrl}/projects/{config.ProjectId}/locations/{config.Location}";
            var url = $"{endpoint}:predict?key={config.ApiKey}";

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Veo API error: {Error}", error);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var operationResponse = JsonSerializer.Deserialize<VeoLongRunningResponse>(responseContent);

            return operationResponse?.Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Veo API");
            return null;
        }
    }

    public async Task<VeoLongRunningResponse?> GetOperationAsync(
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetActiveConfigAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();
            var url = $"{config.ApiBaseUrl}/{operationName}?key={config.ApiKey}";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<VeoLongRunningResponse>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operation status");
            return null;
        }
    }

    public async Task<byte[]> DownloadVideoAsync(
        string videoUri,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        return await client.GetByteArrayAsync(videoUri, cancellationToken);
    }

    private async Task<Lumo.Infrastructure.Entities.VeoConfig> GetActiveConfigAsync(CancellationToken cancellationToken)
    {
        return await _veoConfigRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Veo: Brak aktywnej konfiguracji.");
    }
}
