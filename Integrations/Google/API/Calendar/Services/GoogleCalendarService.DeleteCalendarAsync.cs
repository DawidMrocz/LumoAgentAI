namespace Integrations.Google.API.Calendar.Services
{
    internal sealed partial class GoogleCalendarService
    {
        public async Task DeleteCalendarAsync(int userId, string calendarId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(calendarId))
                throw new ArgumentException("Id kalendarza jest wymagane.");

            if (calendarId.Equals("primary", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Nie można usunąć głównego kalendarza Google.");

            var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);

            await service.Calendars.Delete(calendarId).ExecuteAsync(cancellationToken);
        }
    }
}
