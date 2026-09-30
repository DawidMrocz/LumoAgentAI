using Lumo.Api.Services.AgentTools.Google;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Lumo.Api.Agents.CalendarAgent;

public sealed class CalendarAgent : ICalendarAgent
{
    private readonly IChatClient _chatClient;
    private readonly CalendarAgentTools _tools;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Lazy<AIAgent> _agent;

    public string Name => "CalendarAgent";

    public string Description =>
        "Specjalista od Google Calendar. Obsługuje kalendarze, wydarzenia, " +
        "spotkania, terminy, uczestników i dostępność użytkownika.";

    public CalendarAgent(
        IChatClient chatClient,
        CalendarAgentTools tools,
        ILoggerFactory loggerFactory)
    {
        _chatClient = chatClient;
        _tools = tools;
        _loggerFactory = loggerFactory;

        _agent = new Lazy<AIAgent>(
            CreateAgent,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private AIAgent CreateAgent()
    {
        IList<AITool> tools =
        [
            .. _tools.GetTools()
        ];

        var options = new ChatClientAgentOptions
        {
            Name = Name,

            Description = Description,

            ChatOptions = new ChatOptions
            {
                Instructions =
                """
                Jesteś CalendarAgent — specjalistą Lumo od Google Calendar.

                Wszystkie informacje o rzeczywistych kalendarzach i wydarzeniach
                pobieraj za pomocą dostępnych narzędzi.

                Wszystkie operacje tworzenia, aktualizacji i usuwania
                MUSZĄ zostać wykonane za pomocą odpowiedniego narzędzia.

                NIGDY nie twierdź, że:
                - wydarzenie zostało utworzone,
                - wydarzenie zostało zmienione,
                - wydarzenie zostało usunięte,
                - kalendarz został usunięty,

                jeżeli odpowiednie narzędzie nie zostało faktycznie wywołane
                i nie zwróciło informacji o sukcesie.

                Nie symuluj wykonania operacji.

                Nigdy nie zgaduj calendarId ani eventId.

                Jeśli użytkownik poda nazwę kalendarza, np. B1,
                najpierw użyj GetGoogleCalendars i ustal jego prawdziwe Id.

                Nazwa kalendarza nie jest calendarId.
                """,

                Tools = _tools.GetTools().ToList(),

                ToolMode = ChatToolMode.RequireAny,

                AllowMultipleToolCalls = true,

                Temperature = 0.1f,

                MaxOutputTokens = 1000
            }
        };

        return _chatClient.AsAIAgent(
            options,
            _loggerFactory);
    }

    public async Task<string> ExecuteAsync(
    string instruction,
    CancellationToken cancellationToken = default)
    {
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Warsaw");

        var fullInstruction =
            $"""
        AKTUALNY CZAS:
        {now:yyyy-MM-dd HH:mm:ss zzz}

        AKTUALNA DATA:
        {now:yyyy-MM-dd}

        STREFA CZASOWA:
        Europe/Warsaw

        WAŻNE:
        Wszystkie określenia względne takie jak:
        - dzisiaj
        - jutro
        - pojutrze
        - wczoraj
        - za tydzień
        - w przyszły poniedziałek

        interpretuj względem podanej powyżej aktualnej daty.

        Nigdy nie zgaduj aktualnego roku ani daty.

        ZADANIE UŻYTKOWNIKA:
        {instruction}
        """;

        var response = await _agent.Value.RunAsync(
            fullInstruction,
            cancellationToken: cancellationToken);

        return response.Text ?? string.Empty;
    }
}