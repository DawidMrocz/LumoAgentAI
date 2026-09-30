using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace Lumo.Api.Services.RAG.TextExtractor;

public sealed class WordTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string extension)
    {
        return extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        cancellationToken.ThrowIfCancellationRequested();

        using var document = WordprocessingDocument.Open(
            stream,
            false);

        var body = document.MainDocumentPart?.Document.Body;

        if (body is null)
            return Task.FromResult(string.Empty);

        var builder = new StringBuilder();

        foreach (var paragraph in body.Elements<Paragraph>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = paragraph.InnerText;

            if (!string.IsNullOrWhiteSpace(text))
                builder.AppendLine(text);
        }

        return Task.FromResult(builder.ToString());
    }
}