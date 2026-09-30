using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lumo.Api.Services.RAG.Embeddings;

public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _model;

    public OllamaEmbeddingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;

        _model = configuration["Ollama:EmbedModel"]
            ?? throw new InvalidOperationException(
                "Ollama:EmbedModel is not configured.");
    }

    public async Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var client = _httpClientFactory.CreateClient("ollama");

        var payload = new EmbeddingRequest
        {
            Model = _model,
            Input = text
        };

        using var response = await client.PostAsJsonAsync(
            "/embeddings",
            payload,
            cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ollama embedding request failed. " +
                $"StatusCode: {response.StatusCode}. " +
                $"Response: {responseContent}");
        }

        var result =
            JsonSerializer.Deserialize<EmbeddingResponse>(
                responseContent);

        if (result?.Data is null || result.Data.Count == 0)
        {
            throw new InvalidOperationException(
                "Ollama response does not contain embedding data.");
        }

        return result.Data[0].Embedding;
    }

    private sealed class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = default!;

        [JsonPropertyName("input")]
        public string Input { get; init; } = default!;
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; init; } = [];
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; init; } = [];
    }
}