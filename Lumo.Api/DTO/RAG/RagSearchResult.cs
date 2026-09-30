namespace Lumo.Api.DTO.RAG
{
    public sealed class RagSearchResult
    {
        public string Text { get; init; } = string.Empty;

        public string FileName { get; init; } = string.Empty;

        public string DocumentId { get; init; } = string.Empty;

        public int ChunkIndex { get; init; }

        public float Score { get; init; }
    }
}
