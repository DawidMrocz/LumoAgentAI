namespace Lumo.Infrastructure.Entities;

public sealed class MetaConfig
{
    public int Id { get; set; }

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string ConfigId { get; set; } = string.Empty;

    public string Scopes { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
