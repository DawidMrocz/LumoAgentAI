using Google.Apis.Calendar.v3;
using Integrations.Google.API.Calendar.Mapping;
using Integrations.Google.API.Calendar.Models;

namespace Integrations.Google.API.Calendar.Services
{
    internal sealed partial class GoogleCalendarService
    {
        public async Task<IReadOnlyList<CalendarEvent>> FindEventsAsync(int userId, string query, DateTimeOffset? from = null, DateTimeOffset? to = null, string calendarId = "primary", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Tekst wyszukiwania jest wymagany.", nameof(query));
            }

            if (from.HasValue && to.HasValue && to.Value <= from.Value)
            {
                throw new ArgumentException("Data końcowa musi być późniejsza niż data początkowa.");
            }

            var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);
            var request = service.Events.List(calendarId);

            request.Q = query.Trim();
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            if (from.HasValue)
            {
                request.TimeMinDateTimeOffset = from.Value;
            }

            if (to.HasValue)
            {
                request.TimeMaxDateTimeOffset = to.Value;
            }

            var result = new List<CalendarEvent>();

            string? pageToken = null;

            do
            {
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
