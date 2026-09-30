using Lumo.Infrastructure.Entities;

namespace Integrations.Twilio.Services;

public sealed class SendSmsRequest
{
    public string To { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Sender { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public bool IsFlash { get; set; }

    public Dictionary<string, string>? Variables { get; set; }
}

public sealed class SendSmsResponse
{
    public string MessageId { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public int Parts { get; set; }

    public decimal Points { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; }

    public bool Success { get; set; }

    public string? Error { get; set; }
}

public sealed class SendBulkSmsRequest
{
    public List<string> To { get; set; } = [];

    public string Message { get; set; } = string.Empty;

    public string? Sender { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public Dictionary<string, string>? Variables { get; set; }
}

public sealed class SendBulkSmsResponse
{
    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public List<SendSmsResponse> Results { get; set; } = [];

    public decimal TotalPoints { get; set; }
}

public sealed class SmsTemplate
{
    public string Name { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class SmsMessageInfo
{
    public int Id { get; set; }

    public string? ExternalId { get; set; }

    public string To { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public SmsStatus Status { get; set; }

    public int Parts { get; set; }

    public decimal Points { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }
}

public sealed class SmsStatistics
{
    public int TotalMessages { get; set; }

    public int DeliveredCount { get; set; }

    public int FailedCount { get; set; }

    public decimal TotalPoints { get; set; }
}
