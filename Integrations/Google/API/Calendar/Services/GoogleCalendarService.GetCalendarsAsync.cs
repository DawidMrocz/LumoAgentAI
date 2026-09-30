using Integrations.Google.API.Calendar.Models;

namespace Integrations.Google.API.Calendar.Services;

internal sealed partial class GoogleCalendarService
{
    public async Task<IReadOnlyList<GoogleCalendarInfo>> GetCalendarsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);
        var result = new List<GoogleCalendarInfo>();
        string? pageToken = null;

        do {
            
            var request = service.CalendarList.List();
            request.PageToken = pageToken;
            var response = await request.ExecuteAsync(cancellationToken);

            if (response.Items is not null)
            {
                result.AddRange(response.Items.Select(x => new GoogleCalendarInfo
                        {
                            Id = x.Id ?? string.Empty,
                            Name = x.SummaryOverride ?? x.Summary ?? string.Empty,
                            Description = x.Description,
                            TimeZone = x.TimeZone,
                            ColorId = x.ColorId,
                            BackgroundColor = x.BackgroundColor,
                            ForegroundColor = x.ForegroundColor,
                            IsPrimary = x.Primary ?? false,
                            IsSelected = x.Selected ?? false,
                            IsHidden = x.Hidden ?? false,
                            AccessRole = x.AccessRole
                        }));
            }
            pageToken = response.NextPageToken;

        } while (!string.IsNullOrWhiteSpace(pageToken));

        return result;
    }
}