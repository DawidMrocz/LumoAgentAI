using Integrations.Email.Client;
using Integrations.Email.Models;
using Integrations.Email.Templates;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Integrations.Email.Services;

internal sealed class EmailService : IEmailService
{
    private readonly IEmailClient _emailClient;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly IEmailMessageRepository _messageRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailClient emailClient,
        IEmailTemplateEngine templateEngine,
        IEmailTemplateRepository templateRepository,
        IEmailMessageRepository messageRepository,
        ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _templateEngine = templateEngine;
        _templateRepository = templateRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<SendEmailResponse> SendEmailAsync(
        SendEmailRequest request,
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await SendInternalAsync(
            request.To,
            request.Cc,
            request.Bcc,
            request.Subject,
            request.HtmlBody,
            request.Attachments,
            templateId: null,
            templateName: null,
            userId,
            cancellationToken);
    }

    public async Task<SendEmailResponse> SendTemplatedEmailAsync(
        SendTemplatedEmailRequest request,
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TemplateName))
        {
            throw new ArgumentException("Nazwa szablonu nie może być pusta.", nameof(request));
        }

        var template = await _templateRepository.GetByNameAsync(request.TemplateName, cancellationToken)
            ?? throw new ArgumentException($"Nie znaleziono szablonu e-mail: {request.TemplateName}", nameof(request));

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Szablon e-mail '{request.TemplateName}' jest nieaktywny.");
        }

        var variables = request.Variables ?? [];

        var missing = _templateEngine
            .ExtractVariableNames(template.Subject + template.HtmlBody)
            .Where(name => !variables.ContainsKey(name))
            .ToList();

        if (missing.Count > 0)
        {
            _logger.LogWarning(
                "Szablon '{Template}': brak wartości dla zmiennych {Missing} - zostaną wysłane jako {{{{nazwa}}}}.",
                request.TemplateName,
                string.Join(", ", missing));
        }

        var subject = _templateEngine.Render(template.Subject, variables);
        var htmlBody = _templateEngine.Render(template.HtmlBody, variables);

        return await SendInternalAsync(
            request.To,
            request.Cc,
            request.Bcc,
            subject,
            htmlBody,
            request.Attachments,
            template.Id,
            template.Name,
            userId,
            cancellationToken);
    }

    private async Task<SendEmailResponse> SendInternalAsync(
        List<string> to,
        List<string>? cc,
        List<string>? bcc,
        string subject,
        string htmlBody,
        List<EmailAttachment>? attachments,
        int? templateId,
        string? templateName,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (to is null || to.Count == 0)
        {
            throw new ArgumentException("Musisz podać co najmniej jednego odbiorcę (To).", nameof(to));
        }

        var entity = new EmailMessage
        {
            UserId = userId,
            TemplateId = templateId,
            TemplateName = templateName,
            To = string.Join(';', to),
            Cc = cc is { Count: > 0 } ? string.Join(';', cc) : null,
            Bcc = bcc is { Count: > 0 } ? string.Join(';', bcc) : null,
            Subject = subject,
            Body = htmlBody,
            AttachmentNames = attachments is { Count: > 0 }
                ? string.Join(';', attachments.Select(a => a.FileName))
                : null,
            Status = EmailStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _messageRepository.AddAsync(entity, cancellationToken);

        try
        {
            await _emailClient.SendAsync(to, cc, bcc, subject, htmlBody, attachments, cancellationToken);

            entity.Status = EmailStatus.Sent;
            entity.SentAt = DateTimeOffset.UtcNow;

            await _messageRepository.UpdateAsync(entity, cancellationToken);

            return new SendEmailResponse
            {
                MessageId = entity.Id,
                Success = true,
                SentAt = entity.SentAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania e-maila do {To}", entity.To);

            entity.Status = EmailStatus.Failed;
            entity.ErrorMessage = ex.Message;

            await _messageRepository.UpdateAsync(entity, cancellationToken);

            return new SendEmailResponse
            {
                MessageId = entity.Id,
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<EmailTemplateInfo> CreateTemplateAsync(
        CreateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Nazwa szablonu nie może być pusta.", nameof(request));
        }

        var existing = await _templateRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException($"Szablon e-mail o nazwie '{request.Name}' już istnieje.");
        }

        var template = new EmailTemplate
        {
            Name = request.Name,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            Description = request.Description,
            IsActive = true
        };

        await _templateRepository.AddAsync(template, cancellationToken);

        return MapTemplate(template);
    }

    public async Task<EmailTemplateInfo> UpdateTemplateAsync(
        string name,
        UpdateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = await _templateRepository.GetByNameAsync(name, cancellationToken)
            ?? throw new ArgumentException($"Nie znaleziono szablonu e-mail: {name}", nameof(name));

        template.Subject = request.Subject;
        template.HtmlBody = request.HtmlBody;
        template.Description = request.Description;
        template.IsActive = request.IsActive;

        await _templateRepository.UpdateAsync(template, cancellationToken);

        return MapTemplate(template);
    }

    public async Task DeleteTemplateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByNameAsync(name, cancellationToken);

        if (template is null)
        {
            return;
        }

        await _templateRepository.DeleteAsync(template.Id, cancellationToken);
    }

    public async Task<EmailTemplateInfo?> GetTemplateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByNameAsync(name, cancellationToken);

        return template is null ? null : MapTemplate(template);
    }

    public async Task<IReadOnlyList<EmailTemplateInfo>> GetTemplatesAsync(
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var templates = await _templateRepository.GetAllAsync(onlyActive, cancellationToken);

        return templates.Select(MapTemplate).ToList();
    }

    public async Task<IReadOnlyList<EmailMessageInfo>> GetUserMessagesAsync(
        int userId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepository.GetByUserIdAsync(userId, skip, take, cancellationToken);

        return messages.Select(MapMessage).ToList();
    }

    private static EmailTemplateInfo MapTemplate(EmailTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Subject = template.Subject,
        HtmlBody = template.HtmlBody,
        Description = template.Description,
        IsActive = template.IsActive
    };

    private static EmailMessageInfo MapMessage(EmailMessage message) => new()
    {
        Id = message.Id,
        TemplateId = message.TemplateId,
        TemplateName = message.TemplateName,
        To = message.To,
        Cc = message.Cc,
        Bcc = message.Bcc,
        Subject = message.Subject,
        Status = message.Status.ToString(),
        ErrorMessage = message.ErrorMessage,
        CreatedAt = message.CreatedAt,
        SentAt = message.SentAt
    };
}
