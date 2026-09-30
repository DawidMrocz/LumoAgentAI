using Lumo.Api.DTO.RAG;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Lumo.Api.Services.RAG.VectorStore;

public sealed class QdrantVectorStoreService : IQdrantVectorStoreService
{
    private readonly QdrantClient _qdrantClient;

    public QdrantVectorStoreService(QdrantClient qdrantClient)
    {
        _qdrantClient = qdrantClient;
    }

    public async Task EnsureCollectionAsync(
        string collectionName,
        int dimension,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                "Collection name cannot be empty.",
                nameof(collectionName));

        if (dimension <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimension));

        var collections =
            await _qdrantClient.ListCollectionsAsync(cancellationToken);

        var exists = collections.Any(c => c == collectionName);

        if (exists)
            return;

        await _qdrantClient.CreateCollectionAsync(
            collectionName,
            new VectorParams
            {
                Size = (ulong)dimension,
                Distance = Distance.Cosine
            },
            cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(
        string collectionName,
        string userId,
        IReadOnlyCollection<VectorPoint> points,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                "Collection name cannot be empty.",
                nameof(collectionName));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));

        if (points.Count == 0)
            return;

        var qdrantPoints = points
            .Select(point => MapToQdrantPoint(point, userId))
            .ToList();

        await _qdrantClient.UpsertAsync(
            collectionName,
            qdrantPoints,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collectionName,
        string userId,
        float[] vector,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                "Collection name cannot be empty.",
                nameof(collectionName));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));

        if (vector.Length == 0)
            throw new ArgumentException(
                "Vector cannot be empty.",
                nameof(vector));

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "user_id",
                        Match = new Match
                        {
                            Keyword = userId
                        }
                    }
                }
            }
        };

        var results = await _qdrantClient.SearchAsync(
            collectionName,
            vector,
            filter: filter,
            limit: (ulong)limit,
            cancellationToken: cancellationToken);

        return [.. results.Select(MapSearchResult)];
    }

    private static PointStruct MapToQdrantPoint(
        VectorPoint point,
        string userId)
    {
        var qdrantPoint = new PointStruct
        {
            Id = new PointId
            {
                Num = point.Id
            },
            Vectors = point.Vector
        };

        // User scope
        qdrantPoint.Payload["user_id"] = new Value
        {
            StringValue = userId
        };

        foreach (var payload in point.Payload)
        {
            // Nie pozwalamy nadpisać systemowego user_id
            if (payload.Key.Equals(
                    "user_id",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            qdrantPoint.Payload[payload.Key] =
                ConvertToQdrantValue(payload.Value);
        }

        return qdrantPoint;
    }

    private static Value ConvertToQdrantValue(object? value)
    {
        return value switch
        {
            null => new Value(),

            string stringValue => new Value
            {
                StringValue = stringValue
            },

            int intValue => new Value
            {
                IntegerValue = intValue
            },

            long longValue => new Value
            {
                IntegerValue = longValue
            },

            uint uintValue => new Value
            {
                IntegerValue = uintValue
            },

            ulong ulongValue => new Value
            {
                IntegerValue = checked((long)ulongValue)
            },

            bool boolValue => new Value
            {
                BoolValue = boolValue
            },

            float floatValue => new Value
            {
                DoubleValue = floatValue
            },

            double doubleValue => new Value
            {
                DoubleValue = doubleValue
            },

            _ => new Value
            {
                StringValue = value.ToString() ?? string.Empty
            }
        };
    }

    private static VectorSearchResult MapSearchResult(
        ScoredPoint result)
    {
        return new VectorSearchResult
        {
            Text = GetStringPayload(
                result.Payload,
                "text"),

            FileName = GetStringPayload(
                result.Payload,
                "file_name"),

            DocumentId = GetStringPayload(
                result.Payload,
                "document_id"),

            ChunkIndex = GetIntPayload(
                result.Payload,
                "chunk_index"),

            Score = result.Score
        };
    }

    private static string GetStringPayload(
        IReadOnlyDictionary<string, Value> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return string.Empty;

        return value.StringValue;
    }

    private static int GetIntPayload(
        IReadOnlyDictionary<string, Value> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return 0;

        return (int)value.IntegerValue;
    }
}