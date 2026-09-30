using Integrations.Email.Models;
using Lumo.Infrastructure.Repositories;
using System.Net;
using System.Net.Mail;

namespace Integrations.Email.Client;

internal sealed class EmailClient : IEmailClient
{
    private readonly IEmailConfigRepository _configRepository;

    public EmailClient(IEmailConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task SendAsync(
        IReadOnlyList<string> to,
        IReadOnlyList<string>? cc,
        IReadOnlyList<string>? bcc,
        string subject,
        string htmlBody,
        IReadOnlyList<EmailAttachment>? attachments,
        CancellationToken cancellationToken = default)
    {
        if (to is null || to.Count == 0)
        {
            throw new ArgumentException("Musisz podać co najmniej jednego odbiorcę (To).", nameof(to));
        }

        // Limity egzekwujemy TUTAJ, a nie tylko atrybutami na modelach - do tej metody
        // trafiają też wywołania z narzędzi agenta AI, które omijają walidację MVC.
        var totalRecipients = to.Count + (cc?.Count ?? 0) + (bcc?.Count ?? 0);

        if (totalRecipients > EmailLimits.MaxRecipients)
        {
            throw new ArgumentException(
                $"Przekroczono limit odbiorców ({EmailLimits.MaxRecipients}).", nameof(to));
        }

        if (attachments is { Count: > EmailLimits.MaxAttachments })
        {
            throw new ArgumentException(
                $"Przekroczono limit załączników ({EmailLimits.MaxAttachments}).", nameof(attachments));
        }

        if (attachments is { Count: > 0 })
        {
            long total = 0;

            foreach (var attachment in attachments)
            {
                if (attachment.Content.LongLength > EmailLimits.MaxAttachmentBytes)
                {
                    throw new ArgumentException(
                        $"Załącznik '{attachment.FileName}' przekracza limit " +
                        $"{EmailLimits.MaxAttachmentBytes / 1024 / 1024} MB.", nameof(attachments));
                }

                total += attachment.Content.LongLength;
            }

            if (total > EmailLimits.MaxTotalAttachmentBytes)
            {
                throw new ArgumentException(
                    $"Łączny rozmiar załączników przekracza " +
                    $"{EmailLimits.MaxTotalAttachmentBytes / 1024 / 1024} MB.", nameof(attachments));
            }
        }

        var config = await _configRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Email: Brak aktywnej konfiguracji SMTP.");

        if (string.IsNullOrWhiteSpace(config.Host))
        {
            throw new InvalidOperationException("Email: Brak hosta SMTP w konfiguracji.");
        }

        if (string.IsNullOrWhiteSpace(config.FromEmail))
        {
            throw new InvalidOperationException("Email: Brak adresu nadawcy (FromEmail) w konfiguracji.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(config.FromEmail, config.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        foreach (var address in to)
        {
            message.To.Add(address);
        }

        foreach (var address in cc ?? [])
        {
            message.CC.Add(address);
        }

        foreach (var address in bcc ?? [])
        {
            message.Bcc.Add(address);
        }

        foreach (var attachment in attachments ?? [])
        {
            var stream = new MemoryStream(attachment.Content);
            message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
        }

        using var client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl = config.UseSsl,
            Timeout = config.TimeoutSeconds * 1000,
            Credentials = string.IsNullOrWhiteSpace(config.Username)
                ? null
                : new NetworkCredential(config.Username, config.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
