namespace Lumo.Api.Services.Moderation
{
    using OpenAI;
    using OpenAI.Moderations;

    public class OpenAIModerationService : IOpenAIModerationService
    {
        private readonly ModerationClient _client;

        public OpenAIModerationService(
            OpenAIClient openAiClient,
            IConfiguration configuration)
        {
            var model = configuration["OpenAI:ModerationModel"] ?? "omni-moderation-latest";

            _client = openAiClient.GetModerationClient(model);
        }

        public async Task<ModerationResult> ModerateAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ModerationResult
                {
                    IsAllowed = true,
                    IsFlagged = false
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            var result = await _client.ClassifyTextAsync(text, cancellationToken);

            return new ModerationResult
            {
                IsAllowed = !result.Value.Flagged,
                IsFlagged = result.Value.Flagged
            };
        }
    }

    public sealed class ModerationResult
    {
        public bool IsAllowed { get; init; }
        public bool IsFlagged { get; init; }
    }
}
