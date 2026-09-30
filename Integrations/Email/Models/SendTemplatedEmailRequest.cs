using System.ComponentModel.DataAnnotations;

namespace Integrations.Email.Models;

public sealed class SendTemplatedEmailRequest
{
    [Required(ErrorMessage = "Nazwa szablonu jest wymagana.")]
    [MaxLength(200, ErrorMessage = "Nazwa szablonu jest zbyt długa.")]
    public required string TemplateName { get; init; }

    [Required(ErrorMessage = "Lista odbiorców jest wymagana.")]
    [MinLength(1, ErrorMessage = "Podaj co najmniej jednego odbiorcę.")]
    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu Do.")]
    public required List<string> To { get; init; }

    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu DW.")]
    public List<string>? Cc { get; init; }

    [MaxLength(EmailLimits.MaxRecipients, ErrorMessage = "Zbyt wielu odbiorców w polu UDW.")]
    public List<string>? Bcc { get; init; }

    public required Dictionary<string, string> Variables { get; init; }

    [MaxLength(EmailLimits.MaxAttachments, ErrorMessage = "Zbyt wiele załączników.")]
    public List<EmailAttachment>? Attachments { get; init; }
}
