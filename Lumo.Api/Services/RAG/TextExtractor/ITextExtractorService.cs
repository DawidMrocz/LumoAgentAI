namespace Lumo.Api.Services.RAG.TextExtractor
{
    public interface ITextExtractorService
    {
        Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
