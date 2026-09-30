namespace Integrations.Google.API.Calendar;

public static class CalendarDescriptions
{
    // ============================================================
    // CALENDAR
    // ============================================================

    public const string CalendarId =
        "Identyfikator kalendarza Google. " +
        "Dla głównego kalendarza można użyć wartości 'primary'. " +
        "Nazwa kalendarza nie jest jego calendarId. " +
        "Jeżeli znana jest tylko nazwa kalendarza, należy najpierw pobrać listę kalendarzy i ustalić jego rzeczywiste Id.";

    public const string CalendarName =
        "Czytelna nazwa kalendarza widoczna dla użytkownika, np. Praca, Prywatny lub B1.";

    public const string CalendarDescription =
        "Opcjonalny opis kalendarza Google.";

    public const string CalendarTimeZone =
        "Strefa czasowa kalendarza w formacie IANA, np. Europe/Warsaw.";

    public const string CalendarColorId =
        "Identyfikator koloru kalendarza Google.";

    public const string CalendarBackgroundColor =
        "Kolor tła kalendarza Google.";

    public const string CalendarForegroundColor =
        "Kolor tekstu kalendarza Google.";

    public const string CalendarIsPrimary =
        "Określa, czy jest to główny kalendarz użytkownika Google.";

    public const string CalendarIsSelected =
        "Określa, czy kalendarz jest aktualnie wybrany do wyświetlania użytkownikowi.";

    public const string CalendarIsHidden =
        "Określa, czy kalendarz jest ukryty na liście kalendarzy użytkownika.";

    public const string CalendarAccessRole =
        "Poziom dostępu użytkownika do kalendarza Google, np. owner, writer, reader lub freeBusyReader.";


    // ============================================================
    // EVENT - BASIC
    // ============================================================

    public const string EventId =
        "Identyfikator wydarzenia Google Calendar. " +
        "Jeżeli nie jest znany, należy najpierw wyszukać lub pobrać właściwe wydarzenie.";

    public const string EventCustomId =
        "Opcjonalny własny identyfikator nowego wydarzenia. " +
        "Jeżeli nie zostanie podany, Google wygeneruje identyfikator automatycznie.";

    public const string EventTitle =
        "Tytuł lub krótka nazwa wydarzenia.";

    public const string EventDescription =
        "Opcjonalny opis wydarzenia. " +
        "Nie należy tworzyć treści opisu, jeżeli użytkownik jej nie podał i nie wynika jednoznacznie z zadania.";

    public const string EventLocation =
        "Opcjonalna lokalizacja wydarzenia, np. adres, nazwa miejsca lub sala. " +
        "Nie należy wymyślać lokalizacji, jeżeli nie została podana.";

    public const string EventStatus =
        "Status wydarzenia. Dozwolone wartości Google Calendar to confirmed, tentative lub cancelled.";

    public const string EventVisibility =
        "Widoczność wydarzenia. Dozwolone wartości to default, public, private lub confidential.";

    public const string EventTransparency =
        "Określa, czy wydarzenie blokuje czas użytkownika. " +
        "Wartość opaque oznacza zajęty czas, a transparent oznacza dostępny czas.";

    public const string EventHtmlLink =
        "Adres URL prowadzący do wydarzenia w Google Calendar.";

    public const string RecurringEventId =
        "Identyfikator nadrzędnego wydarzenia cyklicznego, jeżeli wydarzenie jest jedną z jego instancji.";


    // ============================================================
    // EVENT - DATE AND TIME
    // ============================================================

    public const string StartTime =
        "Data i godzina rozpoczęcia wydarzenia posiadającego konkretną godzinę. " +
        "Nie używaj jednocześnie ze StartDate.";

    public const string EndTime =
        "Data i godzina zakończenia wydarzenia posiadającego konkretną godzinę. " +
        "Musi być późniejsza niż StartTime.";

    public const string StartDate =
        "Data rozpoczęcia wydarzenia całodniowego. " +
        "Używaj wyłącznie dla wydarzeń całodniowych i nie używaj jednocześnie ze StartTime.";

    public const string EndDate =
        "Data zakończenia wydarzenia całodniowego. " +
        "W Google Calendar jest to data wyłączna. " +
        "Przykładowo wydarzenie całodniowe tylko 10 maja ma StartDate = 10 maja i EndDate = 11 maja.";

    public const string StartTimeZone =
        "Strefa czasowa rozpoczęcia wydarzenia w formacie IANA, np. Europe/Warsaw.";

    public const string EndTimeZone =
        "Strefa czasowa zakończenia wydarzenia w formacie IANA, np. Europe/Warsaw.";

    public const string IsAllDay =
        "Określa, czy wydarzenie jest wydarzeniem całodniowym.";


    // ============================================================
    // ATTENDEES
    // ============================================================

    public const string Attendees =
        "Lista uczestników wydarzenia. " +
        "Dodawaj uczestników wyłącznie wtedy, gdy ich dane zostały podane przez użytkownika " +
        "lub uzyskane z wiarygodnego narzędzia lub źródła. " +
        "Nigdy nie wymyślaj uczestników ani adresów e-mail. " +
        "Jeżeli użytkownik nie wskazał uczestników, pozostaw listę pustą.";

    public const string AttendeeEmail =
        "Rzeczywisty adres e-mail uczestnika wydarzenia. " +
        "Musi pochodzić od użytkownika lub z wiarygodnego źródła. " +
        "Nigdy nie wymyślaj adresów e-mail i nie używaj przykładowych adresów typu example.com.";

    public const string AttendeeDisplayName =
        "Opcjonalna nazwa wyświetlana uczestnika, np. imię i nazwisko.";

    public const string AttendeeResponseStatus =
        "Status odpowiedzi uczestnika na zaproszenie. " +
        "Możliwe wartości Google Calendar to needsAction, declined, tentative lub accepted.";

    public const string AttendeeIsOrganizer =
        "Określa, czy uczestnik jest organizatorem wydarzenia.";

    public const string AttendeeIsSelf =
        "Określa, czy uczestnik reprezentuje aktualnie zalogowanego użytkownika Google.";

    public const string AttendeeOptional =
        "Określa, czy obecność uczestnika na wydarzeniu jest opcjonalna.";

    public const string AttendeeResource =
        "Określa, czy uczestnik reprezentuje zasób, np. salę konferencyjną.";

    public const string AttendeeComment =
        "Opcjonalny komentarz uczestnika dotyczący wydarzenia.";

    public const string AttendeeAdditionalGuests =
        "Liczba dodatkowych gości reprezentowanych przez tego uczestnika.";


    // ============================================================
    // ORGANIZER
    // ============================================================

    public const string OrganizerEmail =
        "Adres e-mail organizatora wydarzenia.";


    // ============================================================
    // RECURRENCE
    // ============================================================

    public const string Recurrence =
        "Lista reguł cykliczności zgodnych z RFC5545. " +
        "Przykład cotygodniowego wydarzenia w poniedziałek: RRULE:FREQ=WEEKLY;BYDAY=MO.";


    // ============================================================
    // REMINDERS
    // ============================================================

    public const string Reminders =
        "Konfiguracja przypomnień wydarzenia Google Calendar.";

    public const string ReminderUseDefault =
        "Określa, czy wydarzenie ma używać domyślnych przypomnień ustawionych dla danego kalendarza.";

    public const string ReminderOverrides =
        "Lista własnych przypomnień używanych zamiast domyślnych ustawień kalendarza.";

    public const string ReminderMethod =
        "Sposób dostarczenia przypomnienia. " +
        "Najczęściej używane wartości Google Calendar to popup lub email.";

    public const string ReminderMinutes =
        "Liczba minut przed rozpoczęciem wydarzenia, kiedy przypomnienie ma zostać wysłane lub wyświetlone.";


    // ============================================================
    // COLORS / LABELS
    // ============================================================

    public const string EventColorId =
        "Identyfikator koloru wydarzenia Google Calendar.";

    public const string EventLabelId =
        "Identyfikator etykiety wydarzenia Google Calendar. " +
        "Użycie tego pola wymaga ustawienia EventLabelVersion = 1.";


    // ============================================================
    // EVENT TYPE
    // ============================================================

    public const string EventType =
        "Typ wydarzenia Google Calendar. " +
        "Możliwe wartości obejmują m.in. default, birthday, focusTime, outOfOffice oraz workingLocation.";

    public const string BirthdayProperties =
        "Dodatkowe właściwości wydarzenia urodzinowego. " +
        "Używane wyłącznie, gdy EventType ma wartość birthday.";

    public const string FocusTimeProperties =
        "Dodatkowe właściwości wydarzenia typu czas skupienia. " +
        "Używane wyłącznie, gdy EventType ma wartość focusTime.";

    public const string OutOfOfficeProperties =
        "Dodatkowe właściwości wydarzenia poza biurem. " +
        "Używane wyłącznie, gdy EventType ma wartość outOfOffice.";

    public const string WorkingLocationProperties =
        "Informacje o miejscu pracy użytkownika. " +
        "Używane wyłącznie, gdy EventType ma wartość workingLocation.";


    // ============================================================
    // WORKING LOCATION
    // ============================================================

    public const string WorkingLocationType =
        "Typ miejsca pracy. " +
        "Dozwolone wartości to homeOffice, officeLocation lub customLocation.";

    public const string WorkingCustomLocation =
        "Własna lokalizacja użytkownika. " +
        "Używana, gdy typ miejsca pracy ma wartość customLocation.";

    public const string WorkingOfficeLocation =
        "Lokalizacja biurowa użytkownika. " +
        "Używana, gdy typ miejsca pracy ma wartość officeLocation.";

    public const string CustomLocationLabel =
        "Czytelna nazwa własnej lokalizacji, np. Klient, Coworking lub Biuro terenowe.";

    public const string OfficeBuildingId =
        "Identyfikator budynku w konfiguracji Google Workspace.";

    public const string OfficeFloorId =
        "Identyfikator piętra w budynku.";

    public const string OfficeFloorSectionId =
        "Identyfikator sekcji lub części piętra.";

    public const string OfficeDeskId =
        "Identyfikator konkretnego biurka lub stanowiska pracy.";

    public const string OfficeLabel =
        "Czytelna nazwa lokalizacji biurowej, np. Centrala, Biuro Opole lub Pokój 204.";


    // ============================================================
    // GUEST PERMISSIONS
    // ============================================================

    public const string GuestsCanInviteOthers =
        "Określa, czy uczestnicy wydarzenia mogą zapraszać inne osoby.";

    public const string GuestsCanModify =
        "Określa, czy uczestnicy mogą modyfikować wydarzenie.";

    public const string GuestsCanSeeOtherGuests =
        "Określa, czy uczestnicy mogą widzieć listę pozostałych uczestników wydarzenia.";


    // ============================================================
    // TOOL PARAMETERS
    // ============================================================

    public const string SearchQuery =
        "Tekst używany do wyszukiwania wydarzeń, np. fragment tytułu, opisu, lokalizacji lub danych uczestnika.";

    public const string From =
        "Początek przedziału czasu używanego do wyszukiwania lub pobierania wydarzeń.";

    public const string To =
        "Koniec przedziału czasu używanego do wyszukiwania lub pobierania wydarzeń. " +
        "Musi być późniejszy niż początek przedziału.";

    public const string CreateEventRequest =
        "Dane nowego wydarzenia Google Calendar. " +
        "Używaj wyłącznie informacji podanych przez użytkownika lub uzyskanych z wiarygodnych narzędzi. " +
        "Nie wymyślaj brakujących danych.";

    public const string UpdateEventRequest =
        "Dane wydarzenia, które mają zostać zmienione. " +
        "Modyfikuj tylko pola wymagane przez użytkownika. " +
        "Nie wymyślaj wartości dla pozostałych pól.";

    // ============================================================
    // UPDATE EVENT
    // ============================================================

    public const string UpdateEventTitle =
        "Nowy tytuł wydarzenia. Null oznacza brak zmiany.";

    public const string UpdateEventStart =
        "Nowa data i godzina rozpoczęcia wydarzenia. Null oznacza brak zmiany.";

    public const string UpdateEventEnd =
        "Nowa data i godzina zakończenia wydarzenia. Null oznacza brak zmiany.";

    public const string UpdateEventDescription =
        "Nowy opis wydarzenia. Null oznacza brak zmiany. " +
        "Nie wymyślaj opisu, jeżeli użytkownik nie poprosił o jego zmianę.";

    public const string UpdateEventLocation =
        "Nowa lokalizacja wydarzenia. Null oznacza brak zmiany. " +
        "Nie wymyślaj lokalizacji, jeżeli użytkownik jej nie podał.";

    public const string UpdateEventAttendees =
        "Nowa lista adresów e-mail uczestników wydarzenia. " +
        "Null oznacza brak zmiany listy uczestników. " +
        "Pusta lista oznacza usunięcie wszystkich uczestników. " +
        "Dodawaj wyłącznie rzeczywiste adresy e-mail podane przez użytkownika " +
        "lub uzyskane z wiarygodnego źródła. " +
        "Nigdy nie wymyślaj uczestników ani adresów e-mail.";

    // ============================================================
    // OUT OF OFFICE
    // ============================================================

    public const string OutOfOfficeAutoDeclineMode =
        "Sposób automatycznego odrzucania kolidujących spotkań. " +
        "Określa, które nowe lub istniejące spotkania mają być automatycznie odrzucane podczas nieobecności.";

    public const string OutOfOfficeDeclineMessage =
        "Wiadomość wysyłana do organizatora spotkania przy jego automatycznym odrzuceniu. " +
        "Nie wymyślaj treści wiadomości, jeśli użytkownik jej nie podał.";

    // ============================================================
    // FOCUS TIME
    // ============================================================

    public const string FocusTimeAutoDeclineMode =
        "Sposób automatycznego odrzucania kolidujących spotkań podczas czasu skupienia.";

    public const string FocusTimeDeclineMessage =
        "Wiadomość wysyłana przy automatycznym odrzuceniu kolidującego spotkania. " +
        "Nie wymyślaj treści wiadomości, jeśli użytkownik jej nie podał.";

    public const string FocusTimeChatStatus =
        "Status Google Chat ustawiany podczas czasu skupienia.";

    // ============================================================
    // BIRTHDAY
    // ============================================================

    public const string BirthdayType =
        "Typ specjalnego wydarzenia. " +
        "Dozwolone wartości obejmują m.in. birthday, anniversary, custom, other lub self. " +
        "Ustaw wartość zgodnie z rzeczywistym typem wydarzenia i nie zgaduj jej, jeśli użytkownik jej nie określił.";

    // ============================================================
    // ATTENDEE OUTPUT
    // ============================================================

    public const string AttendeeEmailOutput =
        "Adres e-mail uczestnika wydarzenia. " +
        "Służy do identyfikacji uczestnika i obsługi zaproszeń.";

    public const string AttendeeDisplayNameOutput =
        "Nazwa wyświetlana uczestnika, np. imię i nazwisko.";

    public const string AttendeeResponseStatusOutput =
        "Status odpowiedzi uczestnika na zaproszenie. " +
        "Możliwe wartości to m.in. needsAction, accepted, tentative lub declined.";

    // ============================================================
    // CALENDAR EVENT OUTPUT
    // ============================================================

    public const string EventIdOutput =
        "Unikalny identyfikator wydarzenia Google Calendar. " +
        "Używany m.in. do pobierania szczegółów, aktualizacji i usuwania wydarzenia.";

    public const string EventCalendarIdOutput =
        "Identyfikator kalendarza, do którego należy wydarzenie. " +
        "Może być używany razem z EventId podczas dalszych operacji na wydarzeniu.";

    public const string EventTitleOutput =
        "Tytuł lub krótka nazwa wydarzenia.";

    public const string EventDescriptionOutput =
        "Opis wydarzenia, jeżeli został ustawiony.";

    public const string EventLocationOutput =
        "Lokalizacja wydarzenia, np. adres, nazwa miejsca lub sala, jeżeli została ustawiona.";

    public const string EventStartOutput =
        "Data i godzina rozpoczęcia wydarzenia.";

    public const string EventEndOutput =
        "Data i godzina zakończenia wydarzenia.";

    public const string EventIsAllDayOutput =
        "Określa, czy wydarzenie jest wydarzeniem całodniowym bez konkretnej godziny rozpoczęcia i zakończenia.";

    public const string EventStatusOutput =
        "Aktualny status wydarzenia Google Calendar, np. confirmed, tentative lub cancelled.";

    public const string EventOrganizerEmailOutput =
        "Adres e-mail organizatora wydarzenia.";

    public const string EventAttendeesOutput =
        "Lista rzeczywistych uczestników wydarzenia zwróconych przez Google Calendar " +
        "wraz z ich adresami e-mail i statusem odpowiedzi.";

    public const string EventRemindersOutput =
        "Lista przypomnień skonfigurowanych dla wydarzenia.";

    public const string EventRecurringEventIdOutput =
        "Identyfikator nadrzędnego wydarzenia cyklicznego. " +
        "Jest ustawiony, gdy wydarzenie jest jednym z wystąpień serii cyklicznej.";

    public const string EventHtmlLinkOutput =
        "Adres URL prowadzący bezpośrednio do wydarzenia w Google Calendar.";

    // ============================================================
    // GOOGLE CALENDAR INFO OUTPUT
    // ============================================================

    public const string CalendarInfoId =
        "Unikalny identyfikator kalendarza Google. " +
        "Używaj tej wartości jako calendarId podczas pobierania, tworzenia, aktualizacji " +
        "i usuwania wydarzeń. Nie używaj nazwy kalendarza jako calendarId.";

    public const string CalendarInfoName =
        "Nazwa kalendarza widoczna dla użytkownika, np. Praca, Urodziny lub B1.";

    public const string CalendarInfoDescription =
        "Opis kalendarza Google, jeżeli został ustawiony.";

    public const string CalendarInfoTimeZone =
        "Strefa czasowa kalendarza w formacie IANA, np. Europe/Warsaw.";

    public const string CalendarInfoColorId =
        "Identyfikator koloru kalendarza Google.";

    public const string CalendarInfoBackgroundColor =
        "Kolor tła kalendarza zapisany jako wartość koloru, np. #4285f4.";

    public const string CalendarInfoForegroundColor =
        "Kolor tekstu kalendarza, np. #ffffff.";

    public const string CalendarInfoIsPrimary =
        "Określa, czy jest to główny kalendarz użytkownika. " +
        "Dla głównego kalendarza wartość wynosi true.";

    public const string CalendarInfoIsSelected =
        "Określa, czy kalendarz jest zaznaczony do wyświetlania w interfejsie Google Calendar.";

    public const string CalendarInfoIsHidden =
        "Określa, czy kalendarz jest ukryty na liście kalendarzy użytkownika.";

    public const string CalendarInfoAccessRole =
        "Poziom dostępu użytkownika do kalendarza. " +
        "Możliwe wartości to m.in. owner, writer, reader lub freeBusyReader.";

    // ============================================================
    // REMINDER OUTPUT
    // ============================================================

    public const string ReminderMethodOutput =
        "Metoda przypomnienia skonfigurowanego dla wydarzenia, np. popup lub email.";

    public const string ReminderMinutesBeforeOutput =
        "Liczba minut przed rozpoczęciem wydarzenia, kiedy przypomnienie ma zostać uruchomione.";
}