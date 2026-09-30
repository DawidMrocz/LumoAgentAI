using Lumo.Api.DTO.RAG;

namespace Lumo.Api.Services.RAG.Ingestion
{
    public interface IRagIngestionService
    {
        Task<IngestionResult> IngestAsync(Stream stream, string fileName, string collectionName, CancellationToken cancellationToken = default);
    }
}
