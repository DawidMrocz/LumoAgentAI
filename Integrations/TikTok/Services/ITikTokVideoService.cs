namespace Integrations.TikTok.Services;

public interface ITikTokVideoService
{
    Task<PublishVideoResponse> PublishVideoAsync(
        PublishVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<PublishBulkVideosResponse> PublishBulkVideosAsync(
        PublishBulkVideosRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<List<TikTokVideoInfo>> GetUserVideosAsync(
        int userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<TikTokVideoInfo?> GetVideoDetailsAsync(
        int userId,
        string videoId,
        CancellationToken cancellationToken = default);

    Task<TikTokStatistics> GetStatisticsAsync(
        int userId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    Task RefreshVideoStatisticsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}

public sealed class PublishVideoRequest { public string VideoPath { get; set; } = string.Empty; public string? ThumbnailPath { get; set; } public string Caption { get; set; } = string.Empty; public List<string> Hashtags { get; set; } = new(); public TikTokPrivacyLevel PrivacyLevel { get; set; } = TikTokPrivacyLevel.Public; public bool AllowComments { get; set; } = true; public bool AllowDuet { get; set; } = true; public bool AllowStitch { get; set; } = true; public bool IsDraft { get; set; } public DateTimeOffset? ScheduledAt { get; set; } }
public enum TikTokPrivacyLevel { Public, FriendsOnly, Private, MutualFollowFriends }
public sealed class PublishVideoResponse { public bool Success { get; set; } public string? VideoId { get; set; } public string? Status { get; set; } public string? Error { get; set; } public DateTimeOffset? PublishedAt { get; set; } }

public sealed class PublishBulkVideosRequest { public List<PublishVideoRequest> Videos { get; set; } = new(); public int DelayBetweenPublications { get; set; } }
public sealed class PublishBulkVideosResponse { public int SuccessCount { get; set; } public int FailureCount { get; set; } public List<PublishVideoResponse> Results { get; set; } = new(); }
public sealed class TikTokVideoInfo
{
    public string VideoId { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public long ViewCount { get; set; }

    public long LikeCount { get; set; }

    public long CommentCount { get; set; }

    public long ShareCount { get; set; }

    public int? Duration { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
public sealed class TikTokStatistics
{
    public int TotalVideos { get; set; }

    public long TotalViews { get; set; }

    public long TotalLikes { get; set; }

    public long TotalComments { get; set; }

    public long TotalShares { get; set; }

    public double AverageViews { get; set; }

    public double AverageLikes { get; set; }

    public double EngagementRate { get; set; }
}