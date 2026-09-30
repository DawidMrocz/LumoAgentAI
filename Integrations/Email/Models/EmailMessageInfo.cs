namespace Integrations.Email.Models;

public sealed class EmailMessageInfo
{
    public int Id { get; init; }

    public int? TemplateId { get; init; }

    public string? TemplateName { get; init; }

    public required string To { get; init; }

    public string? Cc { get; init; }

    public string? Bcc { get; init; }

    public required string Subject { get; init; }

    public required string Status { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? SentAt { get; init; }
}
