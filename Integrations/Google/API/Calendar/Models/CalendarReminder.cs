using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models;

public sealed record CalendarReminder
{
    [Description(CalendarDescriptions.ReminderMethodOutput)]
    public string Method { get; init; } = string.Empty;

    [Description(CalendarDescriptions.ReminderMinutesBeforeOutput)]
    public int MinutesBefore { get; init; }
}