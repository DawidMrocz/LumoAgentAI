namespace Integrations.Email.Models;

public sealed class SendEmailResponse
{
    public int MessageId { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }

    public DateTimeOffset? SentAt { get; init; }
}
