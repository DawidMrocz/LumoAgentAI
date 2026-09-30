using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models;

public sealed class GoogleCalendarInfo
{
    [Description(CalendarDescriptions.CalendarInfoId)]
    public string Id { get; set; } = string.Empty;

    [Description(CalendarDescriptions.CalendarInfoName)]
    public string Name { get; set; } = string.Empty;

    [Description(CalendarDescriptions.CalendarInfoDescription)]
    public string? Description { get; set; }

    [Description(CalendarDescriptions.CalendarInfoTimeZone)]
    public string? TimeZone { get; set; }

    [Description(CalendarDescriptions.CalendarInfoColorId)]
    public string? ColorId { get; set; }

    [Description(CalendarDescriptions.CalendarInfoBackgroundColor)]
    public string? BackgroundColor { get; set; }

    [Description(CalendarDescriptions.CalendarInfoForegroundColor)]
    public string? ForegroundColor { get; set; }

    [Description(CalendarDescriptions.CalendarInfoIsPrimary)]
    public bool IsPrimary { get; set; }

    [Description(CalendarDescriptions.CalendarInfoIsSelected)]
    public bool IsSelected { get; set; }

    [Description(CalendarDescriptions.CalendarInfoIsHidden)]
    public bool IsHidden { get; set; }

    [Description(CalendarDescriptions.CalendarInfoAccessRole)]
    public string? AccessRole { get; set; }
}