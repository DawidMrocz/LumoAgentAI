using Lumo.Api.DTO.RAG;

namespace Lumo.Api.Services.RAG.Retrieval
{
    public interface IRagSearchService
    {
        Task<IReadOnlyList<RagSearchResult>> SearchAsync(string query, string collectionName, int limit = 5, CancellationToken cancellationToken = default);
    }
}
