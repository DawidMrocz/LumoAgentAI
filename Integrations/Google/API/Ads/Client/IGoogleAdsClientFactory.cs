using Google.Ads.GoogleAds.Lib;

namespace Integrations.Google.API.Ads.Client;

internal interface IGoogleAdsClientFactory
{
    Task<GoogleAdsClient> CreateAsync(int userId, CancellationToken cancellationToken = default);
}
