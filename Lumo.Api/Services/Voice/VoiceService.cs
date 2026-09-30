using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lumo.Api.Services.Voice
{
    public class VoiceService : IVoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VoiceService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string> SpeechToTextAsync(
     byte[] audio,
     CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "OpenAI API key nie jest skonfigurowany.");

            if (audio == null || audio.Length == 0)
                throw new ArgumentException(
                    "Audio jest puste.",
                    nameof(audio));

            // =====================================================
            // DEBUG - zapisujemy dokładnie to, co przyszło z Androida
            // =====================================================

            var debugPath = Path.Combine(
                AppContext.BaseDirectory,
                "debug.wav");

            await File.WriteAllBytesAsync(
                debugPath,
                audio,
                cancellationToken);

            Console.WriteLine(
                $"STT: otrzymano {audio.Length} bytes");

            Console.WriteLine(
                $"STT: debug WAV: {debugPath}");

            // =====================================================
            // OPENAI
            // =====================================================

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey);

            using var content =
                new MultipartFormDataContent();

            using var audioContent =
                new ByteArrayContent(audio);

            audioContent.Headers.ContentType =
                new MediaTypeHeaderValue("audio/wav");

            content.Add(
                audioContent,
                "file",
                "audio.wav");

            // =====================================================
            // MODEL
            // =====================================================

            content.Add(
                new StringContent("gpt-transcribe"),
                "model");

            // =====================================================
            // JĘZYK
            // =====================================================

            content.Add(
                new StringContent("pl"),
                "languages");

            // =====================================================
            // REQUEST
            // =====================================================

            var response = await client.PostAsync(
                "https://api.openai.com/v1/audio/transcriptions",
                content,
                cancellationToken);

            var body =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            Console.WriteLine(
                $"STT HTTP: {(int)response.StatusCode}");

            Console.WriteLine(
                $"STT RESPONSE: {body}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"OpenAI STT error: {body}");
            }

            // =====================================================
            // RESPONSE
            // =====================================================

            var result =
                JsonSerializer.Deserialize<TranscriptionResponse>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            return result?.Text?.Trim() ?? string.Empty;
        }

        public async Task<byte[]> TextToSpeechAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "OpenAI API key nie jest skonfigurowany.");

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey);

            var request = new
            {
                model = "gpt-4o-mini-tts",
                input = text,
                voice = "marin",
                response_format = "mp3"
            };

            var json =
                JsonSerializer.Serialize(request);

            using var content =
                new StringContent(
                    json,
                    System.Text.Encoding.UTF8,
                    "application/json");

            var response = await client.PostAsync(
                "https://api.openai.com/v1/audio/speech",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new InvalidOperationException(
                    $"OpenAI TTS error: {error}");
            }

            return await response.Content
                .ReadAsByteArrayAsync(cancellationToken);
        }

        private sealed class TranscriptionResponse
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";

            [JsonPropertyName("usage")]
            public UsageResponse? Usage { get; set; }
        }

        private sealed class UsageResponse
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "";

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }

            [JsonPropertyName("input_tokens")]
            public int InputTokens { get; set; }

            [JsonPropertyName("input_token_details")]
            public InputTokenDetailsResponse? InputTokenDetails { get; set; }

            [JsonPropertyName("output_tokens")]
            public int OutputTokens { get; set; }
        }

        private sealed class InputTokenDetailsResponse
        {
            [JsonPropertyName("text_tokens")]
            public int TextTokens { get; set; }

            [JsonPropertyName("audio_tokens")]
            public int AudioTokens { get; set; }
        }
    }
}
