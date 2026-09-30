namespace Integrations.Google.API.BusinessProfile.Models;

public sealed class BusinessLocation
{
    public required string Name { get; init; }

    public string? Title { get; init; }

    public string? Address { get; init; }

    public string? PhoneNumber { get; init; }

    public string? WebsiteUri { get; init; }
}
