namespace Lumo.Infrastructure.Entities;

public enum SmsStatus
{
    Queued = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5,
    Expired = 6
}

public sealed class SmsMessage
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? ExternalId { get; set; }

    public string To { get; set; } = string.Empty;

    public string? Sender { get; set; }

    public string Message { get; set; } = string.Empty;

    public SmsStatus Status { get; set; }

    public int Parts { get; set; }

    public decimal Points { get; set; }

    public bool IsFlash { get; set; }

    public bool IsTest { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public int? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }
}
