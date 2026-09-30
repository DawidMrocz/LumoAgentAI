namespace Lumo.Infrastructure.Entities;

public sealed class EmailConfig
{
    public int Id { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseSsl { get; set; } = true;

    public string FromEmail { get; set; } = string.Empty;

    public string? FromName { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
