using System.Text;

namespace Lumo.Api.Services.RAG.Chunking;

public sealed class ChunkingService : IChunkingService
{
    public IReadOnlyList<string> Split(string text, int maxCharacters = 500)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (maxCharacters <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));

        var paragraphs = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (trimmed.Length <= maxCharacters)
            {
                chunks.Add(trimmed);
                continue;
            }

            AddLongParagraph(trimmed, maxCharacters, chunks);
        }

        return chunks;
    }

    private static void AddLongParagraph(string paragraph, int maxCharacters, List<string> chunks)
    {
        var sentences = paragraph.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries);

        var current = new StringBuilder();

        foreach (var sentence in sentences)
        {
            var trimmedSentence = sentence.Trim();

            if (string.IsNullOrWhiteSpace(trimmedSentence))
                continue;

            if (trimmedSentence.Length > maxCharacters)
            {
                FlushCurrent(current, chunks);
                AddByCharacters(trimmedSentence, maxCharacters, chunks);
                continue;
            }

            if (current.Length == 0)
            {
                current.Append(trimmedSentence);
                continue;
            }

            var candidate = $"{current} {trimmedSentence}";

            if (candidate.Length <= maxCharacters)
            {
                current.Append(' ');
                current.Append(trimmedSentence);
            }
            else
            {
                FlushCurrent(current, chunks);
                current.Append(trimmedSentence);
            }
        }

        FlushCurrent(current, chunks);
    }

    private static void AddByCharacters(string text, int maxCharacters, List<string> chunks)
    {
        for (var i = 0; i < text.Length; i += maxCharacters)
        {
            var length = Math.Min(maxCharacters, text.Length - i);
            var chunk = text.Substring(i, length).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);
        }
    }

    private static void FlushCurrent(StringBuilder current, List<string> chunks)
    {
        if (current.Length == 0)
            return;

        chunks.Add(current.ToString().Trim());
        current.Clear();
    }
}