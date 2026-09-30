using System.Text.Json;

namespace Integrations.Meta.Graph
{
    public interface IMetaInstagramService
    {
        Task<JsonElement?> GetMediaAsync(
            int userId,
            string instagramUserId,
            string fields = "id,caption,media_url,timestamp",
            int? limit = null,
            CancellationToken cancellationToken = default);

        Task<string> PublishImageAsync(
            int userId,
            string instagramUserId,
            string imageUrl,
            string caption,
            CancellationToken cancellationToken = default);
    }
}
