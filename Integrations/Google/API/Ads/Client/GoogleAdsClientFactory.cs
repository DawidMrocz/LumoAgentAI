using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Integrations.Google.Auth;
using Lumo.Infrastructure.Repositories;

namespace Integrations.Google.API.Ads.Client;

internal sealed class GoogleAdsClientFactory : IGoogleAdsClientFactory
{
    private readonly IGoogleAccountProvider _googleAccountProvider;
    private readonly IGoogleConfigRepository _googleConfigRepository;

    public GoogleAdsClientFactory(
        IGoogleAccountProvider googleAccountProvider,
        IGoogleConfigRepository googleConfigRepository)
    {
        _googleAccountProvider = googleAccountProvider;
        _googleConfigRepository = googleConfigRepository;
    }

    public async Task<GoogleAdsClient> CreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var config = await _googleConfigRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Google: Brak aktywnej konfiguracji.");

        if (string.IsNullOrWhiteSpace(config.DeveloperToken))
        {
            throw new InvalidOperationException("Google Ads: Brak Developer Token w konfiguracji.");
        }

        var account = await _googleAccountProvider.GetAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Google: Użytkownik nie ma połączonego konta.");

        var adsConfig = new GoogleAdsConfig
        {
            OAuth2ClientId = config.ClientId,
            OAuth2ClientSecret = config.ClientSecret,
            OAuth2RefreshToken = account.RefreshTokenProtected,
            DeveloperToken = config.DeveloperToken
        };

        if (!string.IsNullOrWhiteSpace(config.LoginCustomerId))
        {
            adsConfig.LoginCustomerId = config.LoginCustomerId;
        }

        return new GoogleAdsClient(adsConfig);
    }
}
