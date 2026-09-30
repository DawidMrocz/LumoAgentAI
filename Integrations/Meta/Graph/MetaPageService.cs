using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Meta.Graph
{
    internal sealed class MetaPageService
    : IMetaPageService
    {
        private readonly IMetaGraphClient _graph;

        public MetaPageService(
            IMetaGraphClient graph)
        {
            _graph = graph;
        }

        public async Task<IEnumerable<MetaPage>> GetUserPagesAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _graph.GetAsync<PagesResponse>(
                    "me/accounts",
                    userId,
                    new Dictionary<string, string>
                    {
                        ["fields"] =
                            "id,name,access_token"
                    },
                    cancellationToken);

            return response?.Data
                ?? Enumerable.Empty<MetaPage>();
        }

        public async Task<MetaInstagramBusinessAccount?>
            GetInstagramBusinessAccountAsync(
                int userId,
                string pageId,
                CancellationToken cancellationToken = default)
        {
            var response =
                await _graph.GetAsync<PageResponse>(
                    pageId,
                    userId,
                    new Dictionary<string, string>
                    {
                        ["fields"] =
                            "instagram_business_account"
                    },
                    cancellationToken);

            return response?.InstagramBusinessAccount;
        }

        private sealed class PagesResponse
        {
            public List<MetaPage> Data { get; set; } = [];
        }

        private sealed class PageResponse
        {
            public MetaInstagramBusinessAccount?
                InstagramBusinessAccount
            { get; set; }
        }
    }

    public sealed class MetaPage
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string? AccessToken { get; init; }
    }

    public sealed class MetaInstagramBusinessAccount
    {

    }
}
