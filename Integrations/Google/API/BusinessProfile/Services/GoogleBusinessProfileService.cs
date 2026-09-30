using Google.Apis.BusinessProfilePerformance.v1;
using Google.Apis.Util;
using Integrations.Google.API.BusinessProfile.Models;
using Integrations.Google.Factory;

namespace Integrations.Google.API.BusinessProfile.Services;

internal sealed class GoogleBusinessProfileService : IGoogleBusinessProfileService
{
    private static readonly LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum[] DefaultMetrics =
    [
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.CALLCLICKS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.WEBSITECLICKS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSDIRECTIONREQUESTS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSCONVERSATIONS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSIMPRESSIONSDESKTOPMAPS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSIMPRESSIONSDESKTOPSEARCH,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSIMPRESSIONSMOBILEMAPS,
        LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum.BUSINESSIMPRESSIONSMOBILESEARCH
    ];

    private readonly IGoogleServiceFactory _googleServiceFactory;

    public GoogleBusinessProfileService(IGoogleServiceFactory googleServiceFactory)
    {
        _googleServiceFactory = googleServiceFactory;
    }

    public async Task<IReadOnlyList<BusinessAccount>> ListAccountsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var service = await _googleServiceFactory.CreateBusinessAccountManagementAsync(userId, cancellationToken);

        var response = await service.Accounts.List().ExecuteAsync(cancellationToken);

        return (response.Accounts ?? [])
            .Select(a => new BusinessAccount
            {
                Name = a.Name,
                AccountName = a.AccountName,
                Type = a.Type,
                Role = a.Role,
                VerificationState = a.VerificationState
            })
            .ToList();
    }

    public async Task<IReadOnlyList<BusinessLocation>> ListLocationsAsync(
        int userId,
        string accountName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new ArgumentException(
                "Nazwa konta Google Business Profile nie może być pusta.",
                nameof(accountName));
        }

        var service = await _googleServiceFactory.CreateBusinessInformationAsync(userId, cancellationToken);

        var request = service.Accounts.Locations.List(accountName);
        request.ReadMask = "name,title,storefrontAddress,phoneNumbers,websiteUri";

        var response = await request.ExecuteAsync(cancellationToken);

        return (response.Locations ?? [])
            .Select(l => new BusinessLocation
            {
                Name = l.Name,
                Title = l.Title,
                Address = l.StorefrontAddress?.AddressLines is { Count: > 0 } lines
                    ? string.Join(", ", lines)
                    : null,
                PhoneNumber = l.PhoneNumbers?.PrimaryPhone,
                WebsiteUri = l.WebsiteUri
            })
            .ToList();
    }

    public async Task<IReadOnlyList<LocationMetricPoint>> GetLocationPerformanceAsync(
        int userId,
        string locationName,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locationName))
        {
            throw new ArgumentException(
                "Nazwa lokalizacji Google Business Profile nie może być pusta.",
                nameof(locationName));
        }

        var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = dateFrom ?? to.AddDays(-30);

        var service = await _googleServiceFactory.CreateBusinessProfilePerformanceAsync(userId, cancellationToken);

        var request = service.Locations.FetchMultiDailyMetricsTimeSeries(locationName);
        request.DailyMetricsList = new Repeatable<LocationsResource.FetchMultiDailyMetricsTimeSeriesRequest.DailyMetricsEnum>(DefaultMetrics);
        request.DailyRangeStartDateYear = from.Year;
        request.DailyRangeStartDateMonth = from.Month;
        request.DailyRangeStartDateDay = from.Day;
        request.DailyRangeEndDateYear = to.Year;
        request.DailyRangeEndDateMonth = to.Month;
        request.DailyRangeEndDateDay = to.Day;

        var response = await request.ExecuteAsync(cancellationToken);

        var points = new List<LocationMetricPoint>();

        foreach (var series in response.MultiDailyMetricTimeSeries ?? [])
        {
            foreach (var dailySeries in series.DailyMetricTimeSeries ?? [])
            {
                foreach (var datedValue in dailySeries.TimeSeries?.DatedValues ?? [])
                {
                    if (datedValue.Date?.Year is not { } year ||
                        datedValue.Date?.Month is not { } month ||
                        datedValue.Date?.Day is not { } day)
                    {
                        continue;
                    }

                    points.Add(new LocationMetricPoint
                    {
                        Metric = dailySeries.DailyMetric,
                        Date = new DateOnly(year, month, day),
                        Value = datedValue.Value ?? 0
                    });
                }
            }
        }

        return points;
    }
}
