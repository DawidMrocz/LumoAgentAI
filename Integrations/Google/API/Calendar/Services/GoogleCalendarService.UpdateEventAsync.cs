using Google.Apis.Calendar.v3.Data;
using Integrations.Google.API.Calendar.Mapping;
using Integrations.Google.API.Calendar.Models;
using Integrations.Google.API.Calendar.Models.Requests;

namespace Integrations.Google.API.Calendar.Services;

internal sealed partial class GoogleCalendarService
{
    public async Task<CalendarEvent> UpdateEventAsync(int userId, string eventId, UpdateCalendarEventRequest request, string calendarId = "primary", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("Id wydarzenia jest wymagane.", nameof(eventId));
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.Start.HasValue && request.End.HasValue && request.End.Value <= request.Start.Value)
        {
            throw new ArgumentException("Czas zakończenia musi być późniejszy niż czas rozpoczęcia.");
        }

        var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);
        var patch = new Event();

        if (request.Title is not null)
        {
            patch.Summary = request.Title;
        }

        if (request.Description is not null)
        {
            patch.Description = request.Description;
        }

        if (request.Location is not null)
        {
            patch.Location = request.Location;
        }

        if (request.Start.HasValue)
        {
            patch.Start = new EventDateTime
            {
                DateTimeDateTimeOffset = request.Start
            };
        }

        if (request.End.HasValue)
        {
            patch.End = new EventDateTime
            {
                DateTimeDateTimeOffset = request.End.Value
            };
        }

        if (request.Attendees is not null)
        {
            patch.Attendees = request.Attendees
                .Select(email => new EventAttendee
                {
                    Email = email
                })
                .ToList();
        }

        var updateRequest = service.Events.Patch(patch, calendarId, eventId);
        var updated = await updateRequest.ExecuteAsync(cancellationToken);

        return GoogleCalendarMapper.Map(updated, calendarId);
    }
}