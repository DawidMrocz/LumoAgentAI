using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text;

namespace Lumo.Api.Services.RAG.TextExtractor
{
    public sealed class PowerPointTextExtractor : IFileTextExtractor
    {
        public bool CanHandle(string extension)
        {
            return extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase);
        }

        public Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            cancellationToken.ThrowIfCancellationRequested();

            using var presentation = PresentationDocument.Open(stream, false);

            var presentationPart = presentation.PresentationPart;

            if (presentationPart?.Presentation?.SlideIdList is null)
                return Task.FromResult(string.Empty);

            var builder = new StringBuilder();

            foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<SlideId>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);

                foreach (var text in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                    if (!string.IsNullOrWhiteSpace(text.Text))
                        builder.AppendLine(text.Text);

                builder.AppendLine();
            }

            return Task.FromResult(builder.ToString());
        }
    }
}
