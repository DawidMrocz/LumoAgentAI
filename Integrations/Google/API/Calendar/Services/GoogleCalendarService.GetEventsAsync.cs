using Google.Apis.Calendar.v3;
using Integrations.Google.API.Calendar.Mapping;
using Integrations.Google.API.Calendar.Models;

namespace Integrations.Google.API.Calendar.Services
{
    internal sealed partial class GoogleCalendarService
    {
        public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(int userId, DateTimeOffset from, DateTimeOffset to, string calendarId = "primary", CancellationToken cancellationToken = default)
        {
            if (to <= from)
            {
                throw new ArgumentException("Data końcowa musi być późniejsza niż data początkowa.");
            }

            var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);
            var request = service.Events.List(calendarId);

            request.TimeMinDateTimeOffset = from;
            request.TimeMaxDateTimeOffset = to;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var result = new List<CalendarEvent>();

            string? pageToken = null;

            do {
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync(cancellationToken);

                if (response.Items is not null)
                {
                    result.AddRange(response.Items.Select(x => GoogleCalendarMapper.Map(x, calendarId)));
                }

                pageToken = response.NextPageToken;

            } while (!string.IsNullOrWhiteSpace(pageToken));

            return result;
        }
    }
}
