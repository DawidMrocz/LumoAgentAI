namespace Lumo.Api.Services.Moderation
{
    public interface IOpenAIModerationService
    {
        Task<ModerationResult> ModerateAsync(string text, CancellationToken cancellationToken = default);
    }
}
