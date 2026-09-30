using System.Text;

namespace Lumo.Api.Services.RAG.TextExtractor
{
    public sealed class PlainTextExtractor : IFileTextExtractor
    {
        public bool CanHandle(string extension)
        {
            return extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) || extension.Equals(".md", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}
