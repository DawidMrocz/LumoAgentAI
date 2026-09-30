using Lumo.Api.DTO.RAG;

namespace Lumo.Api.Services.RAG.VectorStore
{
    public interface IQdrantVectorStoreService
    {
        Task EnsureCollectionAsync(
        string collectionName,
        int dimension,
        CancellationToken cancellationToken = default);

        Task UpsertAsync(
            string collectionName,
            string userId,
            IReadOnlyCollection<VectorPoint> points,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
            string collectionName,
            string userId,
            float[] vector,
            int limit,
            CancellationToken cancellationToken = default);
    }
}
