using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models;

public sealed record CalendarEvent
{
    [Description(CalendarDescriptions.EventIdOutput)]
    public string Id { get; init; } = string.Empty;

    [Description(CalendarDescriptions.EventCalendarIdOutput)]
    public string CalendarId { get; init; } = "primary";

    [Description(CalendarDescriptions.EventTitleOutput)]
    public string Title { get; init; } = string.Empty;

    [Description(CalendarDescriptions.EventDescriptionOutput)]
    public string? Description { get; init; }

    [Description(CalendarDescriptions.EventLocationOutput)]
    public string? Location { get; init; }

    [Description(CalendarDescriptions.EventStartOutput)]
    public DateTimeOffset Start { get; init; }

    [Description(CalendarDescriptions.EventEndOutput)]
    public DateTimeOffset End { get; init; }

    [Description(CalendarDescriptions.EventIsAllDayOutput)]
    public bool IsAllDay { get; init; }

    [Description(CalendarDescriptions.EventStatusOutput)]
    public string? Status { get; init; }

    [Description(CalendarDescriptions.EventOrganizerEmailOutput)]
    public string? OrganizerEmail { get; init; }

    [Description(CalendarDescriptions.EventAttendeesOutput)]
    public IReadOnlyList<CalendarAttendee> Attendees { get; init; }
        = Array.Empty<CalendarAttendee>();

    [Description(CalendarDescriptions.EventRemindersOutput)]
    public IReadOnlyList<CalendarReminder> Reminders { get; init; }
        = Array.Empty<CalendarReminder>();

    [Description(CalendarDescriptions.EventRecurringEventIdOutput)]
    public string? RecurringEventId { get; init; }

    [Description(CalendarDescriptions.EventHtmlLinkOutput)]
    public string? HtmlLink { get; init; }
}