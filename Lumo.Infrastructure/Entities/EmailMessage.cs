namespace Lumo.Infrastructure.Entities;

public enum EmailStatus
{
    Queued = 0,
    Sending = 1,
    Sent = 2,
    Failed = 3
}

public sealed class EmailMessage
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? TemplateId { get; set; }

    public string? TemplateName { get; set; }

    public string To { get; set; } = string.Empty;

    public string? Cc { get; set; }

    public string? Bcc { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? AttachmentNames { get; set; }

    public EmailStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }
}
