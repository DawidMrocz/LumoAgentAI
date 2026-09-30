using Integrations.Google.API.Calendar;
using Integrations.Google.API.Calendar.Models.Requests;
using Lumo.Api.Services.AgentUserContext;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lumo.Api.Services.AgentTools.Google;

public sealed class CalendarAgentTools : IAgentToolProvider
{
    private readonly IGoogleCalendarService _calendarService;
    private readonly IAgentUserContextService _userContext;

    public CalendarAgentTools(
        IGoogleCalendarService calendarService,
        IAgentUserContextService userContext)
    {
        _calendarService = calendarService;
        _userContext = userContext;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(
            GetEventsAsync,
            name: "GetCalendarEvents",
            description: "Pobiera wydarzenia z kalendarza Google użytkownika w określonym przedziale czasu.");

        yield return AIFunctionFactory.Create(
            GetEventAsync,
            name: "GetCalendarEvent",
            description: "Pobiera szczegóły konkretnego wydarzenia z kalendarza Google.");

        yield return AIFunctionFactory.Create(
            FindEventsAsync,
            name: "FindCalendarEvents",
            description: "Wyszukuje wydarzenia w kalendarzu Google po nazwie, opisie, lokalizacji lub uczestniku.");

        yield return AIFunctionFactory.Create(
            CreateEventAsync,
            name: "CreateCalendarEvent",
            description: "Tworzy nowe wydarzenie w kalendarzu Google.");

        yield return AIFunctionFactory.Create(
            UpdateEventAsync,
            name: "UpdateCalendarEvent",
            description: "Aktualizuje istniejące wydarzenie w kalendarzu Google.");

        yield return AIFunctionFactory.Create(
            DeleteEventAsync,
            name: "DeleteCalendarEvent",
            description: "Usuwa wydarzenie z kalendarza Google.");

        yield return AIFunctionFactory.Create(
            GetCalendarsAsync,
            name: "GetGoogleCalendars",
            description: "Pobiera listę kalendarzy Google dostępnych dla użytkownika.");

        yield return AIFunctionFactory.Create(
            DeleteCalendarAsync,
            name: "DeleteGoogleCalendar",
            description: "Trwale usuwa wskazany kalendarz Google użytkownika. Użyj tylko wtedy, gdy użytkownik wyraźnie chce usunąć cały kalendarz. Nie używaj do usuwania pojedynczego wydarzenia ani kalendarza głównego primary.");
    }

    private async Task<string> GetEventsAsync(
        [Description("Początek przedziału czasu, od którego mają zostać pobrane wydarzenia.")] DateTimeOffset from,
        [Description("Koniec przedziału czasu, do którego mają zostać pobrane wydarzenia.")] DateTimeOffset to,
        [Description("Rzeczywiste ID kalendarza Google (nie nazwa wyświetlana). Użyj \"primary\" dla głównego kalendarza użytkownika, w innym wypadku pobierz ID przez GetGoogleCalendars.")] string calendarId = "primary")
    {
        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var events = await _calendarService.GetEventsAsync(userId, from, to, calendarId);
            return events.Count == 0 ? "Brak wydarzeń w podanym okresie." : JsonSerializer.Serialize(events);
        }
        catch (Exception ex)
        {
            return $"Nie udało się pobrać wydarzeń z kalendarza: {ex.Message}";
        }
    }

    private async Task<string> GetEventAsync(
        [Description("ID konkretnego wydarzenia w kalendarzu Google.")] string eventId,
        [Description("Rzeczywiste ID kalendarza Google (nie nazwa wyświetlana). Użyj \"primary\" dla głównego kalendarza użytkownika, w innym wypadku pobierz ID przez GetGoogleCalendars.")] string calendarId = "primary")
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return "Nie podano identyfikatora wydarzenia.";

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var calendarEvent = await _calendarService.GetEventAsync(userId, eventId, calendarId);
            return calendarEvent is null ? "Nie znaleziono wydarzenia." : JsonSerializer.Serialize(calendarEvent);
        }
        catch (Exception ex)
        {
            return $"Nie udało się pobrać wydarzenia: {ex.Message}";
        }
    }

    private async Task<string> FindEventsAsync(
        [Description("Fraza wyszukiwania dopasowywana do nazwy, opisu, lokalizacji lub uczestników wydarzenia.")] string query,
        [Description("Opcjonalny początek przedziału czasu ograniczającego wyszukiwanie.")] DateTimeOffset? from = null,
        [Description("Opcjonalny koniec przedziału czasu ograniczającego wyszukiwanie.")] DateTimeOffset? to = null,
        [Description("Rzeczywiste ID kalendarza Google (nie nazwa wyświetlana). Użyj \"primary\" dla głównego kalendarza użytkownika, w innym wypadku pobierz ID przez GetGoogleCalendars.")] string calendarId = "primary")
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Nie podano tekstu wyszukiwania.";

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var events = await _calendarService.FindEventsAsync(userId, query, from, to, calendarId);
            return events.Count == 0 ? "Nie znaleziono pasujących wydarzeń." : JsonSerializer.Serialize(events);
        }
        catch (Exception ex)
        {
            return $"Nie udało się wyszukać wydarzeń: {ex.Message}";
        }
    }

    private async Task<string> CreateEventAsync(
        [Description("Dane nowego wydarzenia (tytuł, czas rozpoczęcia i zakończenia, uczestnicy, lokalizacja itd.).")] CreateCalendarEventRequest request,
        [Description("Rzeczywiste ID kalendarza Google, w którym ma zostać utworzone wydarzenie (nie nazwa wyświetlana). Pobierz je przez GetGoogleCalendars, jeśli nieznane.")] string calendarId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(calendarId))
            return "Nie podano identyfikatora kalendarza.";

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var created = await _calendarService.CreateEventAsync(userId, request, calendarId);
            return JsonSerializer.Serialize(created);
        }
        catch (Exception ex)
        {
            return $"Nie udało się utworzyć wydarzenia: {ex.Message}";
        }
    }

    private async Task<string> UpdateEventAsync(
        [Description("ID istniejącego wydarzenia, które ma zostać zaktualizowane.")] string eventId,
        [Description("Nowe dane wydarzenia, które mają zastąpić dotychczasowe.")] UpdateCalendarEventRequest request,
        [Description("Rzeczywiste ID kalendarza Google (nie nazwa wyświetlana). Użyj \"primary\" dla głównego kalendarza użytkownika, w innym wypadku pobierz ID przez GetGoogleCalendars.")] string calendarId = "primary")
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return "Nie podano identyfikatora wydarzenia.";

        ArgumentNullException.ThrowIfNull(request);

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var updated = await _calendarService.UpdateEventAsync(userId, eventId, request, calendarId);
            return JsonSerializer.Serialize(updated);
        }
        catch (Exception ex)
        {
            return $"Nie udało się zaktualizować wydarzenia: {ex.Message}";
        }
    }

    private async Task<string> DeleteEventAsync(
        [Description("ID wydarzenia, które ma zostać usunięte.")] string eventId,
        [Description("Rzeczywiste ID kalendarza Google, z którego ma zostać usunięte wydarzenie (nie nazwa wyświetlana). Pobierz je przez GetGoogleCalendars, jeśli nieznane.")] string calendarId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return "Nie podano identyfikatora wydarzenia.";

        if (string.IsNullOrWhiteSpace(calendarId))
            return "Nie podano identyfikatora kalendarza.";

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            await _calendarService.DeleteEventAsync(userId, eventId, calendarId);
            return "Wydarzenie zostało usunięte z kalendarza.";
        }
        catch (Exception ex)
        {
            return $"Nie udało się usunąć wydarzenia: {ex.Message}";
        }
    }

    private async Task<string> GetCalendarsAsync()
    {
        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            var calendars = await _calendarService.GetCalendarsAsync(userId);
            return calendars.Count == 0 ? "Nie znaleziono kalendarzy Google." : JsonSerializer.Serialize(calendars);
        }
        catch (Exception ex)
        {
            return $"Nie udało się pobrać listy kalendarzy: {ex.Message}";
        }
    }

    private async Task<string> DeleteCalendarAsync(
        [Description("Rzeczywiste ID kalendarza Google do trwałego usunięcia (nie nazwa wyświetlana). Nigdy nie zgaduj — pobierz je przez GetGoogleCalendars.")] string calendarId)
    {
        if (string.IsNullOrWhiteSpace(calendarId))
            return "Nie podano identyfikatora kalendarza.";

        if (!TryGetUserId(out var userId, out var error))
            return error;

        try
        {
            await _calendarService.DeleteCalendarAsync(userId, calendarId);
            return "Kalendarz został usunięty.";
        }
        catch (Exception ex)
        {
            return $"Nie udało się usunąć kalendarza: {ex.Message}";
        }
    }

    private bool TryGetUserId(out int userId, out string error)
    {
        var raw = _userContext.UserId;

        if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out userId))
        {
            userId = 0;
            error = "Nie udało się ustalić użytkownika.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
