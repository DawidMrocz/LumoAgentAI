using Google.Apis.BusinessProfilePerformance.v1;
using Google.Apis.Calendar.v3;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.MyBusinessAccountManagement.v1;
using Google.Apis.MyBusinessBusinessInformation.v1;

namespace Integrations.Google.Factory
{
    public interface IGoogleServiceFactory
    {
        Task<GmailService> CreateGmailAsync(int userId, CancellationToken cancellationToken = default);
        Task<CalendarService> CreateCalendarAsync(int userId, CancellationToken cancellationToken = default);
        Task<DriveService> CreateDriveAsync(int userId, CancellationToken cancellationToken = default);
        Task<MyBusinessAccountManagementService> CreateBusinessAccountManagementAsync(int userId, CancellationToken cancellationToken = default);
        Task<MyBusinessBusinessInformationService> CreateBusinessInformationAsync(int userId, CancellationToken cancellationToken = default);
        Task<BusinessProfilePerformanceService> CreateBusinessProfilePerformanceAsync(int userId, CancellationToken cancellationToken = default);
    }
}
