using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models.Requests;

public sealed record UpdateCalendarEventRequest
{
    [Description(CalendarDescriptions.UpdateEventTitle)]
    public string? Title { get; init; }

    [Description(CalendarDescriptions.UpdateEventStart)]
    public DateTimeOffset? Start { get; init; }

    [Description(CalendarDescriptions.UpdateEventEnd)]
    public DateTimeOffset? End { get; init; }

    [Description(CalendarDescriptions.UpdateEventDescription)]
    public string? Description { get; init; }

    [Description(CalendarDescriptions.UpdateEventLocation)]
    public string? Location { get; init; }

    [Description(CalendarDescriptions.UpdateEventAttendees)]
    public IReadOnlyList<string>? Attendees { get; init; }
}