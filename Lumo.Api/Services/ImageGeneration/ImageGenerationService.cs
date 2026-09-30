using OpenAI.Images;

namespace Lumo.Api.Services.ImageGeneration;

public class OpenAIImageGenerationService : IImageGenerationService
{
    private readonly ImageClient _client;

    public OpenAIImageGenerationService(IConfiguration configuration)
    {
   var apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Brak konfiguracji OpenAI:ApiKey.");
        }

        _client = new ImageClient(
            model: "gpt-image-2",
            apiKey: apiKey);
    }

    public async Task<byte[]> GenerateAsync(
        string prompt,
        string? size = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException(
                "Prompt nie może być pusty.",
                nameof(prompt));
        }

        var imageSize = size?.ToLowerInvariant() switch
        {
            "1024x1024" => GeneratedImageSize.W1024xH1024,
            //"1024x1536" => GeneratedImageSize.W1024xH1536,
            //"1536x1024" => GeneratedImageSize.W1536xH1024,

            // Domyślnie
            _ => GeneratedImageSize.W1024xH1024
        };

        var options = new ImageGenerationOptions
        {
            Size = imageSize
        };

        var result = await _client.GenerateImageAsync(
            prompt,
            options,
            cancellationToken);

        return result.Value.ImageBytes.ToArray();
    }
}