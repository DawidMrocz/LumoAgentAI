using Integrations.Google.API.Calendar;
using System.ComponentModel;

public sealed class CalendarOutOfOfficePropertiesRequest
{
    [Description(CalendarDescriptions.OutOfOfficeAutoDeclineMode)]
    public string? AutoDeclineMode { get; set; }

    [Description(CalendarDescriptions.OutOfOfficeDeclineMessage)]
    public string? DeclineMessage { get; set; }
}