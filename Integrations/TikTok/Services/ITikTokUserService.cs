namespace Integrations.TikTok.Services;

public interface ITikTokUserService
{
    Task<TikTokUserInfo?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default);
}

public sealed class TikTokUserInfo
{
    public string OpenId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public bool IsVerified { get; set; }

    public long FollowerCount { get; set; }

    public long FollowingCount { get; set; }

    public long LikesCount { get; set; }

    public int VideoCount { get; set; }
}
