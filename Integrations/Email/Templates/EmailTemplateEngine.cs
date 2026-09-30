using System.Text.RegularExpressions;

namespace Integrations.Email.Templates;

internal sealed partial class EmailTemplateEngine : IEmailTemplateEngine
{
    [GeneratedRegex(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}")]
    private static partial Regex VariablePattern();

    public string Render(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template ?? string.Empty;
        }

        return VariablePattern().Replace(template, match =>
        {
            var name = match.Groups[1].Value;

            return variables.TryGetValue(name, out var value)
                ? value
                : match.Value;
        });
    }

    public IReadOnlyList<string> ExtractVariableNames(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return [];
        }

        return VariablePattern()
            .Matches(template)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
