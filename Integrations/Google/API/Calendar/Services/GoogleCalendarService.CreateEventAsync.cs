using Integrations.Google.API.Calendar.Mapping;
using Integrations.Google.API.Calendar.Models;
using Integrations.Google.API.Calendar.Models.Requests;

namespace Integrations.Google.API.Calendar.Services;

internal sealed partial class GoogleCalendarService
{
    public async Task<CalendarEvent> CreateEventAsync(int userId, CreateCalendarEventRequest request, string calendarId = "primary", CancellationToken cancellationToken = default)
    {
        ValidateCreateEventRequest(request);

        var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);
        calendarId = string.IsNullOrWhiteSpace(calendarId) ? "primary" : calendarId;
        var googleEvent = GoogleCalendarCreateEventMapper.Map(request);
        var insertRequest = service.Events.Insert(googleEvent, calendarId);

        if (!string.IsNullOrWhiteSpace(request.EventLabelId))
        {
            insertRequest.EventLabelVersion = 1;
        }

        var created = await insertRequest.ExecuteAsync(cancellationToken);

        return GoogleCalendarMapper.Map(created, calendarId);
    }
}