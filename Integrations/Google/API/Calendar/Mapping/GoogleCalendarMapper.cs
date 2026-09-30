using Google.Apis.Calendar.v3.Data;
using Integrations.Google.API.Calendar.Models;
using System.Globalization;

namespace Integrations.Google.API.Calendar.Mapping;

public static class GoogleCalendarMapper
{
    public static CalendarEvent Map(Event source, string calendarId)
    {
        return new CalendarEvent
        {
            Id = source.Id ?? string.Empty,
            CalendarId = calendarId,
            Title = source.Summary ?? string.Empty,
            Description = source.Description,
            Location = source.Location,
            Start = MapDateTime(source.Start),
            End = MapDateTime(source.End),
            IsAllDay = !string.IsNullOrWhiteSpace(source.Start?.Date),
            Status = source.Status,
            OrganizerEmail = source.Organizer?.Email,
            RecurringEventId = source.RecurringEventId,
            HtmlLink = source.HtmlLink,
            Attendees = source.Attendees?.Select(x => new CalendarAttendee
                {
                    Email = x.Email ?? string.Empty,
                    DisplayName = x.DisplayName,
                    ResponseStatus = x.ResponseStatus,
                    IsOrganizer = x.Organizer ?? false,
                    IsSelf = x.Self ?? false
                }).ToList() ?? []      
        };
    }


    private static DateTimeOffset MapDateTime(EventDateTime? value)
    {
        if (value?.DateTimeDateTimeOffset.HasValue == true)
        {
            return value.DateTimeDateTimeOffset.Value;
        }

        if (!string.IsNullOrWhiteSpace(value?.Date))
        {
            var date = DateOnly.ParseExact(value.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }

        throw new InvalidOperationException("Nie można odczytać daty wydarzenia Google.");
    }
}