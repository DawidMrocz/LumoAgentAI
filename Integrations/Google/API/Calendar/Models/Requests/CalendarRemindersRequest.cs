using Integrations.Google.API.Calendar;
using System.ComponentModel;

public sealed class CalendarRemindersRequest
{
    [Description(CalendarDescriptions.ReminderUseDefault)]
    public bool UseDefault { get; set; } = true;

    [Description(CalendarDescriptions.ReminderOverrides)]
    public List<CalendarReminderRequest>? Overrides { get; set; }
}


public sealed class CalendarReminderRequest
{
    [Description(CalendarDescriptions.ReminderMethod)]
    public string Method { get; set; } = "popup";

    [Description(CalendarDescriptions.ReminderMinutes)]
    public int Minutes { get; set; }
}