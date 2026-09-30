namespace Integrations.Email.Models;

public sealed class EmailAttachment
{
    public required string FileName { get; init; }

    public required byte[] Content { get; init; }

    public string ContentType { get; init; } = "application/octet-stream";
}
