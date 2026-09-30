using Google.Apis.BusinessProfilePerformance.v1;
using Google.Apis.Calendar.v3;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.MyBusinessAccountManagement.v1;
using Google.Apis.MyBusinessBusinessInformation.v1;
using Google.Apis.Services;
using Integrations.Google.Auth;

namespace Integrations.Google.Factory;

internal sealed class GoogleServiceFactory : IGoogleServiceFactory
{
    private const string ApplicationName = "Lumo";

    private readonly IGoogleCredentialProvider _credentialProvider;

    public GoogleServiceFactory(
        IGoogleCredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }

    private async Task<BaseClientService.Initializer> CreateInitializerAsync(int userId, CancellationToken cancellationToken)
    {
        var credential = await _credentialProvider.GetCredentialAsync(userId, cancellationToken);

        return new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        };
    }

    public async Task<GmailService> CreateGmailAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new GmailService(initializer);
    }

    public async Task<CalendarService> CreateCalendarAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new CalendarService(initializer);
    }

    public async Task<DriveService> CreateDriveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new DriveService(initializer);
    }

    public async Task<MyBusinessAccountManagementService> CreateBusinessAccountManagementAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new MyBusinessAccountManagementService(initializer);
    }

    public async Task<MyBusinessBusinessInformationService> CreateBusinessInformationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new MyBusinessBusinessInformationService(initializer);
    }

    public async Task<BusinessProfilePerformanceService> CreateBusinessProfilePerformanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        var initializer = await CreateInitializerAsync(userId, cancellationToken);
        return new BusinessProfilePerformanceService(initializer);
    }

}