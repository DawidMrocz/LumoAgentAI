using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Models.Requests
{
    public class CreateCalendarEventRequest
    {
        [Description(CalendarDescriptions.EventCustomId)]
        public string? Id { get; set; }

        [Description(CalendarDescriptions.EventTitle)]
        public string Title { get; set; } = string.Empty;

        [Description(CalendarDescriptions.EventDescription)]
        public string? Description { get; set; }

        [Description(CalendarDescriptions.EventLocation)]
        public string? Location { get; set; }


        [Description(CalendarDescriptions.StartTime)]
        public DateTimeOffset? StartTime { get; set; }

        [Description(CalendarDescriptions.EndTime)]
        public DateTimeOffset? EndTime { get; set; }

        [Description(CalendarDescriptions.StartDate)]
        public DateOnly? StartDate { get; set; }

        [Description(CalendarDescriptions.EndDate)]
        public DateOnly? EndDate { get; set; }

        [Description(CalendarDescriptions.StartTimeZone)]
        public string? StartTimeZone { get; set; }

        [Description(CalendarDescriptions.EndTimeZone)]
        public string? EndTimeZone { get; set; }


        [Description(CalendarDescriptions.Attendees)]
        public List<CalendarAttendeeRequest> Attendees { get; set; } = [];


        [Description(CalendarDescriptions.EventColorId)]
        public string? ColorId { get; set; }

        [Description(CalendarDescriptions.EventLabelId)]
        public string? EventLabelId { get; set; }


        [Description(CalendarDescriptions.EventType)]
        public string? EventType { get; set; }

        [Description(CalendarDescriptions.BirthdayProperties)]
        public CalendarBirthdayPropertiesRequest? BirthdayProperties { get; set; }

        [Description(CalendarDescriptions.FocusTimeProperties)]
        public CalendarFocusTimePropertiesRequest? FocusTimeProperties { get; set; }

        [Description(CalendarDescriptions.OutOfOfficeProperties)]
        public CalendarOutOfOfficePropertiesRequest? OutOfOfficeProperties { get; set; }

        [Description(CalendarDescriptions.WorkingLocationProperties)]
        public CalendarWorkingLocationPropertiesRequest? WorkingLocationProperties { get; set; }


        [Description(CalendarDescriptions.Recurrence)]
        public List<string>? Recurrence { get; set; }


        [Description(CalendarDescriptions.Reminders)]
        public CalendarRemindersRequest? Reminders { get; set; }


        [Description(CalendarDescriptions.GuestsCanInviteOthers)]
        public bool? GuestsCanInviteOthers { get; set; }

        [Description(CalendarDescriptions.GuestsCanModify)]
        public bool? GuestsCanModify { get; set; }

        [Description(CalendarDescriptions.GuestsCanSeeOtherGuests)]
        public bool? GuestsCanSeeOtherGuests { get; set; }


        [Description(CalendarDescriptions.EventTransparency)]
        public string? Transparency { get; set; }

        [Description(CalendarDescriptions.EventVisibility)]
        public string? Visibility { get; set; }

        [Description(CalendarDescriptions.EventStatus)]
        public string? Status { get; set; }
    }
}