using Integrations.TikTok.Client;
using System.Text.Json.Serialization;

namespace Integrations.TikTok.Services
{
    internal sealed class TikTokUserService : ITikTokUserService
    {
        private const string Fields =
            "open_id,display_name,avatar_url,bio_description,is_verified,follower_count,following_count,likes_count,video_count";

        private readonly ITikTokClient _client;

        public TikTokUserService(ITikTokClient client)
        {
            _client = client;
        }

        public async Task<TikTokUserInfo?> GetUserInfoAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync<TikTokUserInfoResponse>(
                $"/v2/user/info/?fields={Fields}",
                userId,
                cancellationToken);

            var user = response?.Data?.User;

            if (user is null)
                return null;

            return new TikTokUserInfo
            {
                OpenId = user.OpenId,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.BioDescription,
                IsVerified = user.IsVerified,
                FollowerCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                LikesCount = user.LikesCount,
                VideoCount = user.VideoCount
            };
        }

        private sealed class TikTokUserInfoResponse
        {
            [JsonPropertyName("data")] public TikTokUserInfoData? Data { get; set; }
        }

        private sealed class TikTokUserInfoData
        {
            [JsonPropertyName("user")] public TikTokUserInfoDetails? User { get; set; }
        }

        private sealed class TikTokUserInfoDetails
        {
            [JsonPropertyName("open_id")] public string OpenId { get; set; } = string.Empty;
            [JsonPropertyName("display_name")] public string DisplayName { get; set; } = string.Empty;
            [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }
            [JsonPropertyName("bio_description")] public string? BioDescription { get; set; }
            [JsonPropertyName("is_verified")] public bool IsVerified { get; set; }
            [JsonPropertyName("follower_count")] public long FollowerCount { get; set; }
            [JsonPropertyName("following_count")] public long FollowingCount { get; set; }
            [JsonPropertyName("likes_count")] public long LikesCount { get; set; }
            [JsonPropertyName("video_count")] public int VideoCount { get; set; }
        }
    }
}
