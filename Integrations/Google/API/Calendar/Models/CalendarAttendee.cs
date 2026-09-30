using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models;

public sealed record CalendarAttendee
{
    [Description(CalendarDescriptions.AttendeeEmailOutput)]
    public string Email { get; init; } = string.Empty;

    [Description(CalendarDescriptions.AttendeeDisplayNameOutput)]
    public string? DisplayName { get; init; }

    [Description(CalendarDescriptions.AttendeeResponseStatusOutput)]
    public string? ResponseStatus { get; init; }

    [Description(CalendarDescriptions.AttendeeIsOrganizer)]
    public bool IsOrganizer { get; init; }

    [Description(CalendarDescriptions.AttendeeIsSelf)]
    public bool IsSelf { get; init; }
}