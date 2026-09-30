namespace Integrations.Google.API.Ads.Models;

public sealed class AdsCampaign
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string Status { get; init; }

    public string? ChannelType { get; init; }

    public long Clicks { get; init; }

    public long Impressions { get; init; }

    public decimal Cost { get; init; }

    public double Conversions { get; init; }
}
