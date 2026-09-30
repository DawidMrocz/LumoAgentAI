using System.Text;
using UglyToad.PdfPig;

namespace Lumo.Api.Services.RAG.TextExtractor
{
    public sealed class PdfTextExtractor : IFileTextExtractor
    {
        public bool CanHandle(string extension)
        {
            return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        public Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            cancellationToken.ThrowIfCancellationRequested();

            using var document = PdfDocument.Open(stream);

            var pages = document.GetPages();

            var builder = new StringBuilder();

            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = page.Text;

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                builder.AppendLine(text);
                builder.AppendLine();
            }

            return Task.FromResult(builder.ToString());
        }
    }
}
