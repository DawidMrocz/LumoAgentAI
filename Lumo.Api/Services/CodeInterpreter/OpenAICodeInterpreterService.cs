using OpenAI.Responses;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lumo.Api.Services.CodeInterpreter;

#pragma warning disable OPENAI001

public class OpenAICodeInterpreterService : ICodeInterpreterService
{
    private readonly ResponsesClient _client;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAICodeInterpreterService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "Brak konfiguracji OpenAI:ApiKey.");

        _client = new ResponsesClient(_apiKey);
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<CodeInterpreterResult> ExecuteAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException(
                "Prompt nie może być pusty.",
                nameof(prompt));
        }

        var containerConfiguration =
            CodeInterpreterToolContainerConfiguration
                .CreateAutomaticContainerConfiguration();

        var container =
            new CodeInterpreterToolContainer(
                containerConfiguration);

        var codeInterpreterTool =
            ResponseTool.CreateCodeInterpreterTool(container);

        var options = new CreateResponseOptions
        {
            Model = "gpt-5.6"
        };

        options.Tools.Add(codeInterpreterTool);

        options.IncludedProperties.Add(
            IncludedResponseProperty.CodeInterpreterCallOutputs);

        options.InputItems.Add(
            ResponseItem.CreateUserMessageItem(prompt));

        // ---------------------------------------------------------
        // Wykonanie
        // ---------------------------------------------------------

        var result = await _client.CreateResponseAsync(
    options,
    cancellationToken);

        var response = result.Value;

        // Pobieramy pełny response
        var responseDto = await GetResponseAsync(
            response.Id,
            cancellationToken);

        // Szukamy wygenerowanego pliku
        var fileInfo = FindCodeInterpreterFile(
            responseDto);

        if (fileInfo == null)
        {
            return new CodeInterpreterResult
            {
                Text = response.GetOutputText()
            };
        }

        return new CodeInterpreterResult
        {
            Text = response.GetOutputText(),

            ContainerId = fileInfo.ContainerId,

            FileId = fileInfo.FileId,

            FileName = fileInfo.FileName
        };
    }

    private async Task<OpenAIResponseDto> GetResponseAsync(
    string responseId,
    CancellationToken cancellationToken)
    {
        var url =
            $"https://api.openai.com/v1/responses/" +
            $"{Uri.EscapeDataString(responseId)}";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _apiKey);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Nie udało się pobrać odpowiedzi OpenAI. " +
                $"HTTP {(int)response.StatusCode}: {json}");
        }

        var result =
            JsonSerializer.Deserialize<OpenAIResponseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        return result
            ?? throw new InvalidOperationException(
                "Nie udało się zdeserializować odpowiedzi OpenAI.");
    }



    // =============================================================
    // FIND FILE
    // =============================================================

    private static CodeInterpreterFileInfo?
    FindCodeInterpreterFile(OpenAIResponseDto response)
    {
        foreach (var output in response.Output)
        {
            if (!string.Equals(
                    output.Type,
                    "message",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var content in output.Content)
            {
                foreach (var annotation in content.Annotations)
                {
                    if (!string.Equals(
                            annotation.Type,
                            "container_file_citation",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(annotation.ContainerId) ||
                        string.IsNullOrWhiteSpace(annotation.FileId))
                    {
                        continue;
                    }

                    return new CodeInterpreterFileInfo
                    {
                        ContainerId = annotation.ContainerId,
                        FileId = annotation.FileId,
                        FileName =
                            string.IsNullOrWhiteSpace(annotation.FileName)
                                ? "generated-file"
                                : annotation.FileName
                    };
                }
            }
        }

        return null;
    }

    // =============================================================
    // POBIERANIE PLIKU
    // =============================================================

    public async Task<byte[]> DownloadFileAsync(
        string containerId,
        string fileId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerId))
            throw new ArgumentException(
                "ContainerId jest wymagany.",
                nameof(containerId));

        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException(
                "FileId jest wymagany.",
                nameof(fileId));

        var url =
            $"https://api.openai.com/v1/containers/" +
            $"{Uri.EscapeDataString(containerId)}" +
            $"/files/" +
            $"{Uri.EscapeDataString(fileId)}" +
            $"/content";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _apiKey);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                $"Nie udało się pobrać pliku. " +
                $"HTTP {(int)response.StatusCode}: {error}");
        }

        return await response.Content.ReadAsByteArrayAsync(
            cancellationToken);
    }


}

public sealed class CodeInterpreterFileInfo
{
    public string ContainerId { get; set; } = string.Empty;

    public string FileId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
}

public class CodeInterpreterResult
{
    public string Text { get; set; } = string.Empty;

    public byte[]? File { get; set; }

    public string? FileName { get; set; }

    public string? ContainerId { get; set; }

    public string? FileId { get; set; }
}



public sealed class OpenAIResponseDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("output")]
    public List<OpenAIOutputItemDto> Output { get; set; } = [];
}

public sealed class OpenAIOutputItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("content")]
    public List<OpenAIContentItemDto> Content { get; set; } = [];

    [JsonPropertyName("container_id")]
    public string? ContainerId { get; set; }

    [JsonPropertyName("results")]
    public List<OpenAIResultDto> Results { get; set; } = [];
}

public sealed class OpenAIContentItemDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("annotations")]
    public List<OpenAIAnnotationDto> Annotations { get; set; } = [];
}

public sealed class OpenAIAnnotationDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("container_id")]
    public string? ContainerId { get; set; }

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    [JsonPropertyName("filename")]
    public string? FileName { get; set; }
}

public sealed class OpenAIResultDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }
}