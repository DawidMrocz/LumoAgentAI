using System.ComponentModel;
using Integrations.Google.API.Calendar;

public sealed class CalendarWorkingLocationPropertiesRequest
{
    [Description(CalendarDescriptions.WorkingLocationType)]
    public string Type { get; set; } = string.Empty;

    [Description(CalendarDescriptions.WorkingCustomLocation)]
    public CalendarCustomLocationRequest? CustomLocation { get; set; }

    [Description(CalendarDescriptions.WorkingOfficeLocation)]
    public CalendarOfficeLocationRequest? OfficeLocation { get; set; }
}


public sealed class CalendarCustomLocationRequest
{
    [Description(CalendarDescriptions.CustomLocationLabel)]
    public string? Label { get; set; }
}


public sealed class CalendarOfficeLocationRequest
{
    [Description(CalendarDescriptions.OfficeBuildingId)]
    public string? BuildingId { get; set; }

    [Description(CalendarDescriptions.OfficeFloorId)]
    public string? FloorId { get; set; }

    [Description(CalendarDescriptions.OfficeFloorSectionId)]
    public string? FloorSectionId { get; set; }

    [Description(CalendarDescriptions.OfficeDeskId)]
    public string? DeskId { get; set; }

    [Description(CalendarDescriptions.OfficeLabel)]
    public string? Label { get; set; }
}