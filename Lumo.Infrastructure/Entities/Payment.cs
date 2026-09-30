namespace Lumo.Infrastructure.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    Failed = 3,
    Refunded = 4
}

public enum PaymentMethod
{
    Unknown = 0,
    Blik = 181,
    Card = 2,
    Transfer = 3
}

public sealed class Payment
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public required string SessionId { get; set; }

    public string? Token { get; set; }

    public required int Amount { get; set; }

    public required string Currency { get; set; }

    public string? Description { get; set; }

    public required string Email { get; set; }

    public string? ClientName { get; set; }

    public string? Phone { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public PaymentMethod? Method { get; set; }

    public int? P24OrderId { get; set; }

    public string? Statement { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? PaidAt { get; set; }

    public string? Metadata { get; set; }
}
