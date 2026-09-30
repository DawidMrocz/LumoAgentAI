using Integrations.Email.Models;

namespace Integrations.Email.Client;

internal interface IEmailClient
{
    Task SendAsync(
        IReadOnlyList<string> to,
        IReadOnlyList<string>? cc,
        IReadOnlyList<string>? bcc,
        string subject,
        string htmlBody,
        IReadOnlyList<EmailAttachment>? attachments,
        CancellationToken cancellationToken = default);
}
