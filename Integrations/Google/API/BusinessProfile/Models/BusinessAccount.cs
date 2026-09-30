namespace Integrations.Google.API.BusinessProfile.Models;

public sealed class BusinessAccount
{
    public required string Name { get; init; }

    public string? AccountName { get; init; }

    public string? Type { get; init; }

    public string? Role { get; init; }

    public string? VerificationState { get; init; }
}
