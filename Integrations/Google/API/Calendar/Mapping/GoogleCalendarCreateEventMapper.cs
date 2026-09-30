using Google.Apis.Calendar.v3.Data;
using Integrations.Google.API.Calendar.Models.Requests;
using System.Globalization;

namespace Integrations.Google.API.Calendar.Mapping;

public static class GoogleCalendarCreateEventMapper
{
    public static Event Map(CreateCalendarEventRequest request)
    {
        var googleEvent = new Event
        {
            Id = request.Id,
            Summary = request.Title,
            Description = request.Description,
            Location = request.Location,
            Start = MapStart(request),
            End = MapEnd(request),
            ColorId = request.ColorId,
            EventLabelId = request.EventLabelId,
            EventType = request.EventType,
            Status = request.Status,
            Transparency = request.Transparency,
            Visibility = request.Visibility,
            GuestsCanInviteOthers = request.GuestsCanInviteOthers,
            GuestsCanModify = request.GuestsCanModify,
            GuestsCanSeeOtherGuests = request.GuestsCanSeeOtherGuests,
            Recurrence = request.Recurrence,       
            Attendees = MapAttendees(request.Attendees),
            Reminders = MapReminders(request.Reminders),
            BirthdayProperties = MapBirthdayProperties(request.BirthdayProperties),
            FocusTimeProperties = MapFocusTimeProperties(request.FocusTimeProperties),
            OutOfOfficeProperties = MapOutOfOfficeProperties(request.OutOfOfficeProperties),
            WorkingLocationProperties = MapWorkingLocationProperties(request.WorkingLocationProperties)
        };

        return googleEvent;
    }

    public static EventDateTime MapStart(CreateCalendarEventRequest request)
    {
        if (request.StartDate.HasValue)
        {
            return new EventDateTime
            {
                Date = request.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        if (request.StartTime.HasValue)
        {
            return new EventDateTime
            {
                DateTimeDateTimeOffset = request.StartTime.Value,
                TimeZone = request.StartTimeZone
            };
        }

        throw new ArgumentException("Należy podać StartTime lub StartDate.");
    }


    public static EventDateTime MapEnd(CreateCalendarEventRequest request)
    {
        if (request.EndDate.HasValue)
        {
            return new EventDateTime
            {
                Date = request.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        if (request.EndTime.HasValue)
        {
            return new EventDateTime
            {
                DateTimeDateTimeOffset = request.EndTime.Value,
                TimeZone = request.EndTimeZone
            };
        }

        throw new ArgumentException("Należy podać EndTime lub EndDate.");
    }

    private static IList<EventAttendee>? MapAttendees(List<CalendarAttendeeRequest>? attendees)
    {
        if (attendees is not { Count: > 0 })
            return null;

        var invalidAttendee = attendees.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Email));

        if (invalidAttendee is not null)
        {
            throw new ArgumentException("Każdy uczestnik wydarzenia musi posiadać adres e-mail.");
        }

        return attendees.Select(x => new EventAttendee
            {
                Email = x.Email.Trim(),
                DisplayName = x.DisplayName,
                Optional = x.Optional,
                Resource = x.Resource,
                Comment = x.Comment,
                AdditionalGuests = x.AdditionalGuests,
                ResponseStatus = x.ResponseStatus
            }).ToList();
    }

    public static Event.RemindersData? MapReminders(CalendarRemindersRequest? reminders)
    {
        if (reminders is null)
            return null;

        return new Event.RemindersData
        {
            UseDefault = reminders.UseDefault,

            Overrides = reminders.Overrides?.Select(x => new EventReminder
                {
                    Method = x.Method,
                    Minutes = x.Minutes
                })
                .ToList()
        };
    }

    public static EventBirthdayProperties? MapBirthdayProperties(CalendarBirthdayPropertiesRequest? request)
    {
        if (request is null)
            return null;

        return new EventBirthdayProperties
        {
            Type = request.Type
        };
    }

    public static EventFocusTimeProperties? MapFocusTimeProperties(CalendarFocusTimePropertiesRequest? request)
    {
        if (request is null)
            return null;

        return new EventFocusTimeProperties
        {
            AutoDeclineMode = request.AutoDeclineMode,
            DeclineMessage = request.DeclineMessage,
            ChatStatus = request.ChatStatus
        };
    }

    public static EventOutOfOfficeProperties? MapOutOfOfficeProperties(CalendarOutOfOfficePropertiesRequest? request)
    {
        if (request is null)
            return null;

        return new EventOutOfOfficeProperties
        {
            AutoDeclineMode = request.AutoDeclineMode,
            DeclineMessage = request.DeclineMessage
        };
    }

    public static EventWorkingLocationProperties? MapWorkingLocationProperties(CalendarWorkingLocationPropertiesRequest? request)
    {
        if (request is null)
            return null;

        var result = new EventWorkingLocationProperties
            {
                Type = request.Type
            };

        if (request.Type == "homeOffice")
        {
            result.HomeOffice = new object();
        }

        if (request.CustomLocation is not null)
        {
            result.CustomLocation = new EventWorkingLocationProperties.CustomLocationData
                {
                    Label = request.CustomLocation.Label
                };
        }

        if (request.OfficeLocation is not null)
        {
            result.OfficeLocation = new EventWorkingLocationProperties.OfficeLocationData
                {
                    BuildingId = request.OfficeLocation.BuildingId,
                    FloorId = request.OfficeLocation.FloorId,
                    FloorSectionId = request.OfficeLocation.FloorSectionId,
                    DeskId = request.OfficeLocation.DeskId,
                    Label = request.OfficeLocation.Label
                };
        }

        return result;
    }
}