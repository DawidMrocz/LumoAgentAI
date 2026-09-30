using Lumo.Api.DTO.RAG;
using Lumo.Api.Services.AgentUserContext;
using Lumo.Api.Services.RAG.Chunking;
using Lumo.Api.Services.RAG.Embeddings;
using Lumo.Api.Services.RAG.TextExtractor;
using Lumo.Api.Services.RAG.VectorStore;
using System.Security.Cryptography;
using System.Text;

namespace Lumo.Api.Services.RAG.Ingestion;

public sealed class RagIngestionService : IRagIngestionService
{
    private readonly ITextExtractorService _textExtractorService;
    private readonly IChunkingService _chunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IQdrantVectorStoreService _vectorStoreService;
    private readonly IAgentUserContextService _userContext;

    public RagIngestionService(
        ITextExtractorService textExtractorService,
        IChunkingService chunkingService,
        IEmbeddingService embeddingService,
        IQdrantVectorStoreService vectorStoreService,
        IAgentUserContextService userContext)
    {
        _textExtractorService = textExtractorService;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _userContext = userContext;
    }

    public async Task<IngestionResult> IngestAsync(
        Stream stream,
        string fileName,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name cannot be empty.",
                nameof(fileName));

        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                "Collection name cannot be empty.",
                nameof(collectionName));

        var userId = _userContext.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException(
                "Cannot ingest document without an authenticated user.");

        // 1. Extract text from file
        var text = await _textExtractorService.ExtractAsync(
            stream,
            fileName,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(text))
        {
            return new IngestionResult
            {
                Inserted = 0,
                Failed = 0
            };
        }

        // 2. Split text into chunks
        var chunks = _chunkingService.Split(text);

        if (chunks.Count == 0)
        {
            return new IngestionResult
            {
                Inserted = 0,
                Failed = 0
            };
        }

        // 3. Generate document ID
        var documentId = GenerateDocumentId(
            userId,
            fileName);

        var points = new List<VectorPoint>(chunks.Count);

        int? vectorDimension = null;

        // 4. Generate embeddings
        for (var i = 0; i < chunks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = chunks[i];

            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            var embedding =
                await _embeddingService.GenerateAsync(
                    chunk,
                    cancellationToken);

            if (embedding.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Embedding service returned an empty vector " +
                    $"for chunk {i} of file '{fileName}'.");
            }

            // Get vector dimension from first embedding
            vectorDimension ??= embedding.Length;

            // Make sure every embedding has the same dimension
            if (embedding.Length != vectorDimension.Value)
            {
                throw new InvalidOperationException(
                    $"Embedding dimension mismatch. " +
                    $"Expected {vectorDimension.Value}, " +
                    $"but received {embedding.Length} " +
                    $"for chunk {i}.");
            }

            points.Add(new VectorPoint
            {
                Id = ComposeId(
                    userId,
                    fileName,
                    (ulong)i),

                Vector = embedding,

                Payload = new Dictionary<string, object>
                {
                    ["text"] = chunk,
                    ["file_name"] = fileName,
                    ["document_id"] = documentId,
                    ["chunk_index"] = i
                }
            });
        }

        if (points.Count == 0 || !vectorDimension.HasValue)
        {
            return new IngestionResult
            {
                Inserted = 0,
                Failed = 0
            };
        }

        // 5. Ensure collection
        await _vectorStoreService.EnsureCollectionAsync(
            collectionName,
            vectorDimension.Value,
            cancellationToken);

        // 6. Store vectors in user's scope
        await _vectorStoreService.UpsertAsync(
            collectionName,
            userId,
            points,
            cancellationToken);

        return new IngestionResult
        {
            Inserted = points.Count,
            Failed = 0
        };
    }

    private static string GenerateDocumentId(
        string userId,
        string fileName)
    {
        using var sha = SHA256.Create();

        var input = $"{userId}:{fileName}";

        var hash = sha.ComputeHash(
            Encoding.UTF8.GetBytes(input));

        return Convert
            .ToHexString(hash)
            .Substring(0, 16)
            .ToLowerInvariant();
    }

    private static ulong ComposeId(
        string userId,
        string fileName,
        ulong sequence)
    {
        using var sha = SHA256.Create();

        var input = $"{userId}:{fileName}";

        var hash = sha.ComputeHash(
            Encoding.UTF8.GetBytes(input));

        ulong prefix = 0;

        for (var i = 0; i < 8; i++)
            prefix = (prefix << 8) | hash[i];

        return (prefix << 32) |
               (sequence & 0xFFFFFFFFUL);
    }
}