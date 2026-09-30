using Integrations.Google.API.Calendar;
using System.ComponentModel;

public sealed class CalendarFocusTimePropertiesRequest
{
    [Description(CalendarDescriptions.FocusTimeAutoDeclineMode)]
    public string? AutoDeclineMode { get; set; }

    [Description(CalendarDescriptions.FocusTimeDeclineMessage)]
    public string? DeclineMessage { get; set; }

    [Description(CalendarDescriptions.FocusTimeChatStatus)]
    public string? ChatStatus { get; set; }
}