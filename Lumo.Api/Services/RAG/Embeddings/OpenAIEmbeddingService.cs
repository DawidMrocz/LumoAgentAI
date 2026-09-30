using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lumo.Api.Services.RAG.Embeddings;

public sealed class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public OpenAIEmbeddingService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
        var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var model = _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

        var body = new
        {
            input = text,
            model
        };

        using var response = await _http.PostAsJsonAsync("v1/embeddings", body, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
        var embedding = document!.RootElement.GetProperty("data")[0].GetProperty("embedding");
        var vector = new float[embedding.GetArrayLength()];
        var index = 0;
        foreach (var value in embedding.EnumerateArray())
            vector[index++] = value.GetSingle();

        return vector;
    }
}