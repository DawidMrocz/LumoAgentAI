namespace Lumo.Api.Services.RAG.TextExtractor
{
    public sealed class TextExtractorService : ITextExtractorService
    {
        private readonly IReadOnlyList<IFileTextExtractor> _extractors;

        public TextExtractorService(IEnumerable<IFileTextExtractor> extractors)
        {
            _extractors = [.. extractors];
        }

        public async Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            var extension = Path.GetExtension(fileName);

            if (string.IsNullOrWhiteSpace(extension))
                throw new NotSupportedException($"Cannot determine file type for '{fileName}'.");

            var extractor = _extractors.FirstOrDefault(x => x.CanHandle(extension))
                ?? throw new NotSupportedException($"File type '{extension}' is not supported.");

            var text = await extractor.ExtractAsync(stream, cancellationToken);

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException($"No text could be extracted from '{fileName}'.");

            return text.Trim();
        }
    }
}
