namespace Lumo.Api.DTO.RAG
{
    public class IngestionResult
    {
        public string DocumentId { get; init; } = string.Empty;

        public string FileName { get; init; } = string.Empty;

        public int Chunks { get; init; }

        public int Inserted { get; init; }

        public int Failed { get; init; }
    }
}
