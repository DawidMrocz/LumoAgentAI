namespace Lumo.Infrastructure.Entities;

public sealed class GoogleConfig
{
    public int Id { get; set; }

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string Scopes { get; set; } = string.Empty;

    public string? DeveloperToken { get; set; }

    public string? LoginCustomerId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}