using Lumo.Api.DTO.RAG;
using Lumo.Api.Services.AgentUserContext;
using Lumo.Api.Services.RAG.Embeddings;
using Lumo.Api.Services.RAG.VectorStore;

namespace Lumo.Api.Services.RAG.Retrieval;

public sealed class RagSearchService : IRagSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IQdrantVectorStoreService _vectorStoreService;
    private readonly IAgentUserContextService _userContext;

    public RagSearchService(
        IEmbeddingService embeddingService,
        IQdrantVectorStoreService vectorStoreService,
        IAgentUserContextService userContext)
    {
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        string collectionName,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var userId = _userContext.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException(
                "Cannot search RAG without an authenticated user.");

        var embedding =
            await _embeddingService.GenerateAsync(
                query,
                cancellationToken);

        if (embedding.Length == 0)
            return [];

        var results =
            await _vectorStoreService.SearchAsync(
                collectionName,
                userId,
                embedding,
                limit,
                cancellationToken);

        return
        [
            .. results.Select(x => new RagSearchResult
            {
                Text = x.Text,
                FileName = x.FileName,
                DocumentId = x.DocumentId,
                ChunkIndex = x.ChunkIndex,
                Score = x.Score
            })
        ];
    }
}