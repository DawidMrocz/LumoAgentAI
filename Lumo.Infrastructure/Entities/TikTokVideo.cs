namespace Lumo.Infrastructure.Entities;

public enum TikTokVideoStatus
{
    Pending = 0,
    Uploading = 1,
    Processing = 2,
    Published = 3,
    Draft = 4,
    Scheduled = 5,
    Failed = 6,
    Deleted = 7
}

public sealed class TikTokVideo
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? TikTokVideoId { get; set; }

    public string? PublishId { get; set; }

    public string Caption { get; set; } = string.Empty;

    public string? VideoPath { get; set; }

    public string? ThumbnailPath { get; set; }

    public string? VideoUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public TikTokVideoStatus Status { get; set; }

    public string PrivacyLevel { get; set; } = "PUBLIC_TO_EVERYONE";

    public bool AllowComments { get; set; }

    public bool AllowDuet { get; set; }

    public bool AllowStitch { get; set; }

    public bool IsDraft { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public long ViewCount { get; set; }

    public long LikeCount { get; set; }

    public long CommentCount { get; set; }

    public long ShareCount { get; set; }

    public int Duration { get; set; }

    public long FileSize { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
