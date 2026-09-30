using Integrations.Google.API.Calendar.Models;
using Integrations.Google.API.Calendar.Models.Requests;
using System.ComponentModel;

namespace Integrations.Google.API.Calendar
{
    public interface IGoogleCalendarService
    {
        [Description("Pobiera wydarzenia użytkownika z kalendarza Google w określonym przedziale czasu. Użyj do pytań typu 'co mam jutro' lub 'co mam w przyszłym tygodniu'.")]
        Task<IReadOnlyList<CalendarEvent>> GetEventsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Początek zakresu czasu.")]
                DateTimeOffset from,

                [Description("Koniec zakresu czasu.")]
                DateTimeOffset to,

                [Description("Id kalendarza Google. Domyślnie primary.")]
                string calendarId = "primary",
                
                CancellationToken cancellationToken = default
            );

        [Description("Pobiera pojedyncze wydarzenie po jego unikalnym identyfikatorze Google. Przydatne przed edycją, usunięciem lub wyświetleniem szczegółów wydarzenia.")]
        Task<CalendarEvent?> GetEventAsync
            (
            
                [Description("Id użytkownika Lumo.")]
                int userId,
                
                [Description("Unikalny identyfikator wydarzenia.")]
                string eventId,
                
                [Description("Id kalendarza Google. Domyślnie primary.")]
                string calendarId = "primary",
                
                CancellationToken cancellationToken = default
            );

        [Description("Tworzy nowe wydarzenie w kalendarzu. Może zawierać tytuł, opis, lokalizację, uczestników, przypomnienia oraz czas rozpoczęcia i zakończenia. Jeżeli użytkownik nie wskazał kalendarza, użyj 'primary', a jak wskazał sprawdź jego ID po nazwie.")]      
        Task<CalendarEvent> CreateEventAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Dane nowego wydarzenia.")]
                CreateCalendarEventRequest request,

                [Description("Id kalendarza Google, w którym ma zostać utworzone wydarzenie. Domyślnie primary. Jeśli znana jest tylko nazwa kalendarza, najpierw pobierz listę kalendarzy i ustal jego Id.")]
                string calendarId = "primary",
                
                CancellationToken cancellationToken = default
            );

        [Description("Usuwa konkretne wydarzenie z kalendarza na podstawie jego unikalnego identyfikatora ID.")]
        Task DeleteEventAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,
                
                [Description("Unikalny identyfikator wydarzenia.")]
                string eventId,
                
                [Description("Id kalendarza Google. Domyślnie primary.")]
                string calendarId = "primary",
                
                CancellationToken cancellationToken = default
            );

        [Description("Wyszukuje wydarzenia w kalendarzu Google na podstawie tekstu, np. tytułu, opisu, lokalizacji lub uczestnika. Użyj do pytań typu 'znajdź dentystę', 'kiedy mam spotkanie z Anną' lub 'pokaż wydarzenia dotyczące projektu Lumo'.")]
        Task<IReadOnlyList<CalendarEvent>> FindEventsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Tekst wyszukiwania, np. nazwa wydarzenia, osoba, lokalizacja lub fragment opisu.")]
                string query,

                [Description("Opcjonalny początek zakresu czasu wyszukiwania.")]
                DateTimeOffset? from = null,

                [Description("Opcjonalny koniec zakresu czasu wyszukiwania.")]
                DateTimeOffset? to = null,

                [Description("Id kalendarza Google. Domyślnie primary.")]
                string calendarId = "primary",

                CancellationToken cancellationToken = default
            );

        [Description("Aktualizuje istniejące wydarzenie w kalendarzu Google. Użyj, gdy użytkownik chce zmienić tytuł, termin, lokalizację, opis, uczestników lub inne dane wydarzenia.")]
        Task<CalendarEvent> UpdateEventAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Unikalny identyfikator wydarzenia Google.")]
                string eventId,

                [Description("Dane wydarzenia, które mają zostać zmienione.")]
                UpdateCalendarEventRequest request,

                [Description("Id kalendarza Google. Domyślnie primary.")]
                string calendarId = "primary",

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera listę kalendarzy Google dostępnych dla użytkownika, np. kalendarz główny, Urodziny oraz inne dodane kalendarze. " +
                     "Użyj tej metody zawsze, gdy trzeba ustalić jakie kalendarze użytkownik posiada, poznać ich Id, nazwę, typ, uprawnienia lub inne dane potrzebne do wykonania kolejnej operacji. " +
                     "W szczególności użyj jej przed wywołaniem innych metod kalendarza, jeśli nie znasz poprawnego calendarId.")]
        Task<IReadOnlyList<GoogleCalendarInfo>> GetCalendarsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId, 
                
                CancellationToken cancellationToken = default
            );

        [Description("Trwale usuwa kalendarz pomocniczy Google. Nie można używać dla głównego kalendarza primary. Nie zgaduj calendarId na podstawie nazwy kalendarza — najpierw pobierz listę kalendarzy i użyj zwróconego Id.")]
        Task DeleteCalendarAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Id kalendarza Google do usunięcia.")]
                string calendarId,

                CancellationToken cancellationToken = default
            );
    }
}
