using Integrations.Email;
using Integrations.Email.Models;
using Lumo.Api.Services.AgentUserContext;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Lumo.Api.Services.AgentTools.Email;

/// <summary>
/// Provider narzędzi e-mail (SMTP) dla agenta AI
/// </summary>
public sealed class EmailAgentTools : IAgentToolProvider
{
    private readonly IEmailService _emailService;
    private readonly IAgentUserContextService _userContext;

    public EmailAgentTools(
        IEmailService emailService,
        IAgentUserContextService userContext)
    {
        _emailService = emailService;
        _userContext = userContext;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(
            ListEmailTemplatesAsync,
            name: "ListEmailTemplates",
            description: "Pobiera listę dostępnych szablonów e-mail HTML (nazwa, temat, zmienne) zapisanych w bazie danych.");

        yield return AIFunctionFactory.Create(
            SendEmailAsync,
            name: "SendEmail",
            description: "Wysyła dowolny e-mail HTML przez skonfigurowany serwer SMTP, bez użycia szablonu. Obsługuje odbiorców Cc/Bcc podanych jako listy adresów rozdzielone przecinkiem.");

        yield return AIFunctionFactory.Create(
            SendTemplatedEmailAsync,
            name: "SendTemplatedEmail",
            description: "Wysyła e-mail na podstawie szablonu HTML zapisanego w bazie, podstawiając podane zmienne w formacie {{nazwa}}.");
    }

    private async Task<string> ListEmailTemplatesAsync()
    {
        var templates = await _emailService.GetTemplatesAsync(onlyActive: true);

        if (templates.Count == 0)
        {
            return "Brak dostępnych szablonów e-mail.";
        }

        return JsonSerializer.Serialize(templates, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? cc = null,
        string? bcc = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return "Nie podano odbiorcy (to).";
        }

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(htmlBody))
        {
            return "Nie podano tematu lub treści wiadomości.";
        }

        var userId = ResolveUserId();

        var response = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            To = SplitAddresses(to)!,
            Cc = SplitAddresses(cc),
            Bcc = SplitAddresses(bcc),
            Subject = subject,
            HtmlBody = htmlBody
        }, userId);

        return response.Success
            ? $"E-mail wysłany do: {to} (Id: {response.MessageId})."
            : $"Nie udało się wysłać e-maila: {response.Error}";
    }

    private async Task<string> SendTemplatedEmailAsync(
        string templateName,
        string to,
        Dictionary<string, string> variables,
        string? cc = null,
        string? bcc = null)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return "Nie podano nazwy szablonu.";
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            return "Nie podano odbiorcy (to).";
        }

        var userId = ResolveUserId();

        var response = await _emailService.SendTemplatedEmailAsync(new SendTemplatedEmailRequest
        {
            TemplateName = templateName,
            To = SplitAddresses(to)!,
            Cc = SplitAddresses(cc),
            Bcc = SplitAddresses(bcc),
            Variables = variables ?? []
        }, userId);

        return response.Success
            ? $"E-mail (szablon '{templateName}') wysłany do: {to} (Id: {response.MessageId})."
            : $"Nie udało się wysłać e-maila: {response.Error}";
    }

    private int? ResolveUserId()
    {
        var userId = _userContext.UserId;

        return !string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out var userIdInt)
            ? userIdInt
            : null;
    }

    private static List<string>? SplitAddresses(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
