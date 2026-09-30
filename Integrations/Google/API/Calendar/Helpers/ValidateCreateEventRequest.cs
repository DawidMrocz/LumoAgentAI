using Integrations.Google.API.Calendar.Models.Requests;

namespace Integrations.Google.API.Calendar.Services;

internal sealed partial class GoogleCalendarService
{
    private static void ValidateCreateEventRequest(CreateCalendarEventRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Tytuł wydarzenia jest wymagany.");
        }

        var hasDateTime = request.StartTime.HasValue || request.EndTime.HasValue;

        var hasAllDay = request.StartDate.HasValue || request.EndDate.HasValue;

        if (hasDateTime && hasAllDay)
        {
            throw new ArgumentException("Nie można jednocześnie podawać StartTime/EndTime oraz StartDate/EndDate.");
        }

        if (hasDateTime)
        {
            if (!request.StartTime.HasValue || !request.EndTime.HasValue)
            {
                throw new ArgumentException("Należy podać zarówno StartTime, jak i EndTime.");
            }

            if (request.EndTime.Value <= request.StartTime.Value)
            {
                throw new ArgumentException("Czas zakończenia musi być późniejszy niż rozpoczęcia.");
            }

            return;
        }

        if (!request.StartDate.HasValue || !request.EndDate.HasValue)
        {
            throw new ArgumentException("Należy podać StartTime/EndTime albo StartDate/EndDate.");
        }

        if (request.EndDate.Value <= request.StartDate.Value)
        {
            throw new ArgumentException("Data zakończenia musi być późniejsza niż rozpoczęcia.");
        }
    }
}