using Integrations.Google.API.Calendar;
using System.ComponentModel;

public sealed class CalendarBirthdayPropertiesRequest
{
    [Description(CalendarDescriptions.BirthdayType)]
    public string? Type { get; set; }
}