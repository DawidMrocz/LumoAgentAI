namespace Integrations.Google.API.BusinessProfile.Models;

public sealed class LocationMetricPoint
{
    public required string Metric { get; init; }

    public required DateOnly Date { get; init; }

    public long Value { get; init; }
}
