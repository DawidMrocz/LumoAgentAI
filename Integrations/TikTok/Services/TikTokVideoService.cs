using Integrations.TikTok.Client;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Integrations.TikTok.Services
{
    internal sealed class TikTokVideoService : ITikTokVideoService
    {
        private readonly ITikTokClient _client;
        private readonly ITikTokVideoRepository _repository;
        private readonly ILogger<TikTokVideoService> _logger;

        public TikTokVideoService(
            ITikTokClient client,
            ITikTokVideoRepository repository,
            ILogger<TikTokVideoService> logger)
        {
            _client = client;
            _repository = repository;
            _logger = logger;
        }

        public async Task<PublishVideoResponse> PublishVideoAsync(
            PublishVideoRequest request,
            int userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(request.VideoPath))
                {
                    return new PublishVideoResponse
                    {
                        Success = false,
                        Error = "Plik wideo nie został znaleziony."
                    };
                }

                var fileInfo = new FileInfo(request.VideoPath);

                var video = new TikTokVideo
                {
                    UserId = userId,
                    Caption = BuildCaption(request.Caption, request.Hashtags),
                    VideoPath = request.VideoPath,
                    ThumbnailPath = request.ThumbnailPath,
                    Status = TikTokVideoStatus.Uploading,
                    PrivacyLevel = MapPrivacyLevel(request.PrivacyLevel),
                    AllowComments = request.AllowComments,
                    AllowDuet = request.AllowDuet,
                    AllowStitch = request.AllowStitch,
                    IsDraft = request.IsDraft,
                    ScheduledAt = request.ScheduledAt,
                    FileSize = fileInfo.Length,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _repository.AddAsync(video, cancellationToken);

                var uploadId = await InitializeUploadAsync(userId, fileInfo.Length, cancellationToken);

                if (string.IsNullOrWhiteSpace(uploadId))
                    return await FailAsync(video, "Nie udało się zainicjować uploadu.", cancellationToken);

                var uploaded = await UploadVideoAsync(userId, uploadId, request.VideoPath, cancellationToken);

                if (!uploaded)
                    return await FailAsync(video, "Nie udało się wgrać video.", cancellationToken);

                var publishId = await SubmitPublishAsync(userId, uploadId, video, cancellationToken);

                if (string.IsNullOrWhiteSpace(publishId))
                    return await FailAsync(video, "Nie udało się opublikować video.", cancellationToken);

                video.PublishId = publishId;
                video.Status = request.IsDraft ? TikTokVideoStatus.Draft : TikTokVideoStatus.Processing;
                video.PublishedAt = DateTimeOffset.UtcNow;
                video.UpdatedAt = DateTimeOffset.UtcNow;

                await _repository.UpdateAsync(video, cancellationToken);

                return new PublishVideoResponse
                {
                    VideoId = publishId,
                    Status = video.Status.ToString(),
                    Success = true,
                    PublishedAt = video.PublishedAt.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing TikTok video");

                return new PublishVideoResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<PublishBulkVideosResponse> PublishBulkVideosAsync(
            PublishBulkVideosRequest request,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var response = new PublishBulkVideosResponse();

            for (var i = 0; i < request.Videos.Count; i++)
            {
                var result = await PublishVideoAsync(request.Videos[i], userId, cancellationToken);

                response.Results.Add(result);

                if (result.Success)
                    response.SuccessCount++;
                else
                    response.FailureCount++;

                var isLast = i == request.Videos.Count - 1;

                if (!isLast && request.DelayBetweenPublications > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(request.DelayBetweenPublications), cancellationToken);
                }
            }

            return response;
        }

        public async Task<List<TikTokVideoInfo>> GetUserVideosAsync(
            int userId,
            int skip = 0,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            var videos = await _repository.GetByUserIdAsync(userId, skip, take, cancellationToken);

            return videos.Select(MapToInfo).ToList();
        }

        public async Task<TikTokVideoInfo?> GetVideoDetailsAsync(
            int userId,
            string videoId,
            CancellationToken cancellationToken = default)
        {
            var video = await _repository.GetByPublishIdAsync(videoId, cancellationToken);

            if (video is null || video.UserId != userId)
                return null;

            return MapToInfo(video);
        }

        public async Task<TikTokStatistics> GetStatisticsAsync(
            int userId,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default)
        {
            var videos = await _repository.GetPublishedByUserIdAsync(userId, from, to, cancellationToken);

            if (videos.Count == 0)
                return new TikTokStatistics();

            var totalViews = videos.Sum(v => v.ViewCount);
            var totalLikes = videos.Sum(v => v.LikeCount);
            var totalComments = videos.Sum(v => v.CommentCount);
            var totalShares = videos.Sum(v => v.ShareCount);
            var totalEngagement = totalLikes + totalComments + totalShares;

            return new TikTokStatistics
            {
                TotalVideos = videos.Count,
                TotalViews = totalViews,
                TotalLikes = totalLikes,
                TotalComments = totalComments,
                TotalShares = totalShares,
                AverageViews = (double)totalViews / videos.Count,
                AverageLikes = (double)totalLikes / videos.Count,
                EngagementRate = totalViews > 0 ? (double)totalEngagement / totalViews : 0
            };
        }

        public async Task RefreshVideoStatisticsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var videos = await _repository.GetByUserIdAsync(userId, 0, int.MaxValue, cancellationToken);

            var withPublishId = videos.Where(v => !string.IsNullOrWhiteSpace(v.PublishId)).ToList();

            if (withPublishId.Count == 0)
                return;

            var request = new
            {
                filters = new
                {
                    video_ids = withPublishId.Select(v => v.PublishId).ToArray()
                }
            };

            var response = await _client.PostAsync<TikTokVideoQueryResponse>(
                "/v2/video/query/?fields=id,like_count,comment_count,share_count,view_count",
                userId,
                request,
                cancellationToken);

            var apiVideos = response?.Data?.Videos ?? [];

            foreach (var apiVideo in apiVideos)
            {
                var video = withPublishId.FirstOrDefault(v => v.PublishId == apiVideo.Id);

                if (video is null)
                    continue;

                video.ViewCount = apiVideo.ViewCount;
                video.LikeCount = apiVideo.LikeCount;
                video.CommentCount = apiVideo.CommentCount;
                video.ShareCount = apiVideo.ShareCount;

                if (video.Status == TikTokVideoStatus.Processing)
                    video.Status = TikTokVideoStatus.Published;

                video.UpdatedAt = DateTimeOffset.UtcNow;

                await _repository.UpdateAsync(video, cancellationToken);
            }
        }

        private async Task<string?> InitializeUploadAsync(
            int userId,
            long fileSize,
            CancellationToken cancellationToken)
        {
            const long chunkSize = 10 * 1024 * 1024;

            var totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

            var request = new
            {
                source_info = new
                {
                    source = "FILE_UPLOAD",
                    video_size = fileSize,
                    chunk_size = chunkSize,
                    total_chunk_count = totalChunks
                }
            };

            var response = await _client.PostAsync<TikTokInitializeUploadResponse>(
                "/v2/post/publish/inbox/video/init/",
                userId,
                request,
                cancellationToken);

            return response?.Data?.UploadId;
        }

        private async Task<bool> UploadVideoAsync(
            int userId,
            string uploadId,
            string videoPath,
            CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(videoPath);

            var content = new StreamContent(stream);

            content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

            var response = await _client.PutRawAsync(
                $"/v2/post/publish/inbox/video/upload/?upload_id={Uri.EscapeDataString(uploadId)}",
                userId,
                content,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }

        private async Task<string?> SubmitPublishAsync(
            int userId,
            string uploadId,
            TikTokVideo video,
            CancellationToken cancellationToken)
        {
            var request = new
            {
                post_info = new
                {
                    title = video.Caption,
                    privacy_level = video.PrivacyLevel,
                    disable_comment = !video.AllowComments,
                    disable_duet = !video.AllowDuet,
                    disable_stitch = !video.AllowStitch
                },

                source_info = new
                {
                    source = "FILE_UPLOAD",
                    video_id = uploadId
                }
            };

            var response = await _client.PostAsync<TikTokVideoUploadResponse>(
                "/v2/post/publish/video/init/",
                userId,
                request,
                cancellationToken);

            return response?.Data?.PublishId;
        }

        private async Task<PublishVideoResponse> FailAsync(
            TikTokVideo video,
            string error,
            CancellationToken cancellationToken)
        {
            video.Status = TikTokVideoStatus.Failed;
            video.ErrorMessage = error;
            video.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(video, cancellationToken);

            return new PublishVideoResponse
            {
                Success = false,
                Error = error
            };
        }

        private static TikTokVideoInfo MapToInfo(TikTokVideo video) => new()
        {
            VideoId = video.PublishId ?? video.Id.ToString(),
            Caption = video.Caption,
            VideoUrl = video.VideoUrl ?? string.Empty,
            ThumbnailUrl = video.ThumbnailUrl,
            ViewCount = video.ViewCount,
            LikeCount = video.LikeCount,
            CommentCount = video.CommentCount,
            ShareCount = video.ShareCount,
            Duration = video.Duration,
            CreatedAt = video.CreatedAt
        };

        private static string BuildCaption(string caption, List<string> hashtags)
        {
            if (hashtags.Count == 0)
                return caption.Trim();

            var hashtagString = string.Join(
                " ",
                hashtags.Select(h => h.StartsWith('#') ? h : $"#{h}"));

            return $"{caption} {hashtagString}".Trim();
        }

        private static string MapPrivacyLevel(TikTokPrivacyLevel level)
        {
            return level switch
            {
                TikTokPrivacyLevel.Public => "PUBLIC_TO_EVERYONE",
                TikTokPrivacyLevel.FriendsOnly => "MUTUAL_FOLLOW_FRIENDS",
                TikTokPrivacyLevel.Private => "SELF_ONLY",
                TikTokPrivacyLevel.MutualFollowFriends => "MUTUAL_FOLLOW_FRIENDS",
                _ => "PUBLIC_TO_EVERYONE"
            };
        }
    }

    public sealed class TikTokInitializeUploadResponse
    {
        [JsonPropertyName("data")] public TikTokUploadData? Data { get; set; }
        [JsonPropertyName("error")] public TikTokApiError? Error { get; set; }
    }

    public sealed class TikTokUploadData
    {
        [JsonPropertyName("upload_id")] public string? UploadId { get; set; }
        [JsonPropertyName("upload_url")] public string? UploadUrl { get; set; }
    }

    public sealed class TikTokVideoUploadResponse
    {
        [JsonPropertyName("data")] public TikTokVideoUploadData? Data { get; set; }
        [JsonPropertyName("error")] public TikTokApiError? Error { get; set; }
    }

    public sealed class TikTokVideoUploadData
    {
        [JsonPropertyName("publish_id")] public string? PublishId { get; set; }
    }

    public sealed class TikTokApiError
    {
        [JsonPropertyName("code")] public string? Code { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("log_id")] public object? LogId { get; set; }
    }

    public sealed class TikTokVideoQueryResponse
    {
        [JsonPropertyName("data")] public TikTokVideoQueryData? Data { get; set; }
        [JsonPropertyName("error")] public TikTokApiError? Error { get; set; }
    }

    public sealed class TikTokVideoQueryData
    {
        [JsonPropertyName("videos")] public List<TikTokApiVideoStats> Videos { get; set; } = [];
    }

    public sealed class TikTokApiVideoStats
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("like_count")] public long LikeCount { get; set; }
        [JsonPropertyName("comment_count")] public long CommentCount { get; set; }
        [JsonPropertyName("share_count")] public long ShareCount { get; set; }
        [JsonPropertyName("view_count")] public long ViewCount { get; set; }
    }
}
