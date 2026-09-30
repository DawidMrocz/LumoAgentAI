namespace Integrations.Email.Models;

public sealed class EmailTemplateInfo
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public string? Description { get; init; }

    public bool IsActive { get; init; }
}

public sealed class CreateEmailTemplateRequest
{
    public required string Name { get; init; }

    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public string? Description { get; init; }
}

public sealed class UpdateEmailTemplateRequest
{
    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}
