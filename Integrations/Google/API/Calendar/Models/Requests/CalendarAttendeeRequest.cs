using Integrations.Google.API.Calendar;
using System.ComponentModel;

public sealed class CalendarAttendeeRequest
{
    [Description(CalendarDescriptions.AttendeeEmail)]
    public string Email { get; set; } = string.Empty;

    [Description(CalendarDescriptions.AttendeeDisplayName)]
    public string? DisplayName { get; set; }

    [Description(CalendarDescriptions.AttendeeOptional)]
    public bool? Optional { get; set; }

    [Description(CalendarDescriptions.AttendeeResource)]
    public bool? Resource { get; set; }

    [Description(CalendarDescriptions.AttendeeComment)]
    public string? Comment { get; set; }

    [Description(CalendarDescriptions.AttendeeAdditionalGuests)]
    public int? AdditionalGuests { get; set; }

    [Description(CalendarDescriptions.AttendeeResponseStatus)]
    public string? ResponseStatus { get; set; }
}