using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Integrations.Meta.Graph
{
    internal sealed class MetaInstagramService
    : IMetaInstagramService
    {
        private readonly IMetaGraphClient _graph;

        public MetaInstagramService(
            IMetaGraphClient graph)
        {
            _graph = graph;
        }

        public async Task<JsonElement?> GetMediaAsync(
            int userId,
            string instagramUserId,
            string fields = "id,caption,media_url,timestamp",
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string>
            {
                ["fields"] = fields
            };

            if (limit.HasValue)
            {
                query["limit"] =
                    limit.Value.ToString();
            }

            return await _graph.GetAsync<JsonElement>(
                $"{instagramUserId}/media",
                userId,
                query,
                cancellationToken);
        }

        public async Task<string> PublishImageAsync(
            int userId,
            string instagramUserId,
            string imageUrl,
            string caption,
            CancellationToken cancellationToken = default)
        {
            var container =
                await _graph.PostAsync<MediaContainerResponse>(
                    $"{instagramUserId}/media",
                    userId,
                    new Dictionary<string, string>
                    {
                        ["image_url"] = imageUrl,
                        ["caption"] = caption
                    },
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(container?.Id))
            {
                throw new InvalidOperationException(
                    "Meta nie zwróciła creation id.");
            }

            var published =
                await _graph.PostAsync<PublishResponse>(
                    $"{instagramUserId}/media_publish",
                    userId,
                    new Dictionary<string, string>
                    {
                        ["creation_id"] = container.Id
                    },
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(published?.Id))
            {
                throw new InvalidOperationException(
                    "Meta nie zwróciła ID opublikowanego posta.");
            }

            return published.Id;
        }

        private sealed record MediaContainerResponse(
            string? Id);

        private sealed record PublishResponse(
            string? Id);
    }
}
