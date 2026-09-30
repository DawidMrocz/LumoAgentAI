using Google.Ads.GoogleAds.V25.Services;
using Integrations.Google.API.Ads.Client;
using Integrations.Google.API.Ads.Models;
using ServiceTemplates = Google.Ads.GoogleAds.Services;

namespace Integrations.Google.API.Ads.Services;

internal sealed class GoogleAdsService : IGoogleAdsService
{
    private readonly IGoogleAdsClientFactory _clientFactory;

    public GoogleAdsService(IGoogleAdsClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IReadOnlyList<AdsAccount>> ListAccessibleAccountsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientFactory.CreateAsync(userId, cancellationToken);

        var customerService = client.GetService(ServiceTemplates.V25.CustomerService);
        var adsService = client.GetService(ServiceTemplates.V25.GoogleAdsService);

        var listResponse = await customerService.ListAccessibleCustomersAsync(
            new ListAccessibleCustomersRequest());

        const string query =
            "SELECT customer.id, customer.descriptive_name, customer.currency_code, " +
            "customer.time_zone, customer.manager, customer.test_account " +
            "FROM customer LIMIT 1";

        var accounts = new List<AdsAccount>();

        foreach (var resourceName in listResponse.ResourceNames)
        {
            var customerId = resourceName.Split('/')[^1];

            try
            {
                await foreach (var row in adsService.SearchAsync(customerId, query)
                    .WithCancellation(cancellationToken))
                {
                    var customer = row.Customer;

                    accounts.Add(new AdsAccount
                    {
                        CustomerId = customerId,
                        DescriptiveName = customer.DescriptiveName,
                        CurrencyCode = customer.CurrencyCode,
                        TimeZone = customer.TimeZone,
                        IsManager = customer.Manager,
                        IsTestAccount = customer.TestAccount
                    });
                }
            }
            catch (Exception)
            {
                accounts.Add(new AdsAccount
                {
                    CustomerId = customerId
                });
            }
        }

        return accounts;
    }

    public async Task<IReadOnlyList<AdsCampaign>> GetCampaignsAsync(
        int userId,
        string customerId,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Identyfikator konta Google Ads nie może być pusty.",
                nameof(customerId));
        }

        var normalizedCustomerId = new string(customerId.Where(char.IsDigit).ToArray());

        var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = dateFrom ?? to.AddDays(-30);

        var client = await _clientFactory.CreateAsync(userId, cancellationToken);
        var adsService = client.GetService(ServiceTemplates.V25.GoogleAdsService);

        var query =
            "SELECT campaign.id, campaign.name, campaign.status, campaign.advertising_channel_type, " +
            "metrics.clicks, metrics.impressions, metrics.cost_micros, metrics.conversions " +
            "FROM campaign " +
            $"WHERE segments.date BETWEEN '{from:yyyy-MM-dd}' AND '{to:yyyy-MM-dd}' " +
            "ORDER BY campaign.id";

        var campaigns = new List<AdsCampaign>();

        await foreach (var row in adsService.SearchAsync(normalizedCustomerId, query)
            .WithCancellation(cancellationToken))
        {
            campaigns.Add(new AdsCampaign
            {
                Id = row.Campaign.Id,
                Name = row.Campaign.Name,
                Status = row.Campaign.Status.ToString(),
                ChannelType = row.Campaign.AdvertisingChannelType.ToString(),
                Clicks = row.Metrics.Clicks,
                Impressions = row.Metrics.Impressions,
                Cost = row.Metrics.CostMicros / 1_000_000m,
                Conversions = row.Metrics.Conversions
            });
        }

        return campaigns;
    }
}
