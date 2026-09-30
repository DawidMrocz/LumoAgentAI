namespace Lumo.Infrastructure.Entities;

public enum VeoVideoStatus
{
    Pending = 0,
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public sealed class VeoVideo
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string JobId { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string? NegativePrompt { get; set; }

    public string? VideoPath { get; set; }

    public string? VideoUrl { get; set; }

    public string? ThumbnailPath { get; set; }

    public string? ThumbnailUrl { get; set; }

    public VeoVideoStatus Status { get; set; }

    public int Duration { get; set; }

    public string Resolution { get; set; } = "1080p";

    public string AspectRatio { get; set; } = "16:9";

    public int Fps { get; set; }

    public string? Style { get; set; }

    public int? Seed { get; set; }

    public long FileSize { get; set; }

    public string? ReferenceImagePath { get; set; }

    public string? Preset { get; set; }

    public int Progress { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public TimeSpan? GenerationTime { get; set; }

    public string? Metadata { get; set; }
}
