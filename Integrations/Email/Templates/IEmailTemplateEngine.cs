namespace Integrations.Email.Templates;

internal interface IEmailTemplateEngine
{
    string Render(string template, IReadOnlyDictionary<string, string> variables);

    IReadOnlyList<string> ExtractVariableNames(string template);
}
