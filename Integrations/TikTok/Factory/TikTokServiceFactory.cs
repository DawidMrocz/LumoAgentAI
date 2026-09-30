using Integrations.TikTok.Services;

namespace Integrations.TikTok.Factory
{
    internal sealed class TikTokServiceFactory : ITikTokServiceFactory
    {
        private readonly ITikTokVideoService _videoService;

        public TikTokServiceFactory(ITikTokVideoService videoService)
        {
            _videoService = videoService;
        }

        public Task<ITikTokVideoService> CreateVideoServiceAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_videoService);
        }
    }
}
