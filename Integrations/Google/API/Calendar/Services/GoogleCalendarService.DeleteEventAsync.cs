namespace Integrations.Google.API.Calendar.Services
{
    internal sealed partial class GoogleCalendarService
    {
        public async Task DeleteEventAsync(int userId, string eventId, string calendarId = "primary", CancellationToken cancellationToken = default)
        {
            var service = await _googleServiceFactory.CreateCalendarAsync(userId, cancellationToken);           
            await service.Events.Delete(calendarId, eventId).ExecuteAsync(cancellationToken);
        }
    }
}
