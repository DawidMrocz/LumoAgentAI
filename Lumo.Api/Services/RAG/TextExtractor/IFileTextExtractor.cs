namespace Lumo.Api.Services.RAG.TextExtractor
{
    public interface IFileTextExtractor
    {
        bool CanHandle(string extension);
        Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
