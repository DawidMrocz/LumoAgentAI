namespace Integrations.Email.Models;

/// <summary>
/// Twarde limity wysyłki. Chronią przed użyciem API jako przekaźnika spamu
/// oraz przed wyczerpaniem pamięci przez ogromne załączniki.
/// </summary>
public static class EmailLimits
{
    public const int MaxRecipients = 50;

    public const int MaxAttachments = 10;

    public const int MaxBodyLength = 500_000;

    public const long MaxAttachmentBytes = 10L * 1024 * 1024;

    public const long MaxTotalAttachmentBytes = 25L * 1024 * 1024;
}
