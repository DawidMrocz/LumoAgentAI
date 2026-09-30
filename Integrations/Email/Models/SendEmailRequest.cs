using System.ComponentModel.DataAnnotations;

namespace Integrations.Email.Models;

public sealed class SendEmailRequest
{
    [Required(ErrorMessage = "Lista odbiorców jest wymagana.")]
    [MinLength(1, ErrorMessage = "Podaj co najmniej jednego odbiorcę.")]
    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu Do.")]
    public required List<string> To { get; init; }

    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu DW.")]
    public List<string>? Cc { get; init; }

    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu UDW.")]
    public List<string>? Bcc { get; init; }

    [Required(ErrorMessage = "Temat jest wymagany.")]
    [MaxLength(500, ErrorMessage = "Temat może mieć maksymalnie 500 znaków.")]
    public required string Subject { get; init; }

    [Required(ErrorMessage = "Treść wiadomości jest wymagana.")]
    [MaxLength(EmailLimits.MaxBodyLength, ErrorMessage = "Treść wiadomości jest zbyt długa.")]
    public required string HtmlBody { get; init; }

    [MaxLength(EmailLimits.MaxAttachments, ErrorMessage = "Zbyt wiele załączników.")]
    public List<EmailAttachment>? Attachments { get; init; }
}
