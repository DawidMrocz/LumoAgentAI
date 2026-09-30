namespace Integrations.Google.API.Ads.Models;

public sealed class AdsAccount
{
    public required string CustomerId { get; init; }

    public string? DescriptiveName { get; init; }

    public string? CurrencyCode { get; init; }

    public string? TimeZone { get; init; }

    public bool IsManager { get; init; }

    public bool IsTestAccount { get; init; }
}
