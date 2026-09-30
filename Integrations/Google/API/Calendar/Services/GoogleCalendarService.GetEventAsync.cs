using Google;
using Integrations.Google.API.Calendar.Mapping;
using Integrations.Google.API.Calendar.Models;
using System.Net;

namespace Integrations.Google.API.Calendar.Services
{
    internal sealed partial class GoogleCalendarService
    {
        public async Task<CalendarEvent?> GetEventAsync(int userId, string eventId, string calendarId = "primary", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException("Id wydarzenia jest wymagane.", nameof(eventId));
            }

            var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);

            try
            {
                var googleEvent = await service.Events.Get(calendarId, eventId).ExecuteAsync(cancellationToken);
                return GoogleCalendarMapper.Map(googleEvent, calendarId);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
