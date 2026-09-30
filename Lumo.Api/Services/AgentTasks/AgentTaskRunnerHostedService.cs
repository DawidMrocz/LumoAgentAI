using Cronos;
using Lumo.Api.Data.Agent;
using Lumo.Api.Data.User;
using Lumo.Api.Services.Agent;
using Lumo.Api.Services.AgentSessionStore;
using Lumo.Api.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Api.Services.AgentTasks;

public sealed class AgentTaskRunnerHostedService : BackgroundService
{
    private readonly ILogger<AgentTaskRunnerHostedService> _logger;
    private readonly IServiceProvider _services;

    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(30);

    public AgentTaskRunnerHostedService(
        ILogger<AgentTaskRunnerHostedService> logger,
        IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "AgentTaskRunnerHostedService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTasksAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AgentTaskRunnerHostedService error");
            }

            try
            {
                await Task.Delay(
                    PollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation(
            "AgentTaskRunnerHostedService stopped");
    }

    private async Task ProcessTasksAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var agentService =
            scope.ServiceProvider
                .GetRequiredService<IAgentService>();

        var sessionStore =
            scope.ServiceProvider
                .GetRequiredService<IAgentSessionStoreService>();

        var fcmService =
            scope.ServiceProvider
                .GetRequiredService<IFcmService>();

        var nowUtc = DateTime.UtcNow;

        var tasks =
            await db.AgentTasks
                .Where(t =>
                    t.Enabled &&
                    t.NextRunAt != null &&
                    !string.IsNullOrWhiteSpace(t.UserOid) &&
                    t.NextRunAt <= nowUtc)
                .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            try
            {
                await ExecuteTaskAsync(
                    task,
                    agentService,
                    sessionStore,
                    fcmService,
                    cancellationToken);

                task.LastRunAt = nowUtc;

                if (!string.IsNullOrWhiteSpace(
                        task.CronExpression))
                {
                    task.NextRunAt =
                        CalculateNextRun(
                            task,
                            nowUtc);
                }
                else
                {
                    task.NextRunAt = null;
                    task.Enabled = false;
                }

                task.UpdatedAt = nowUtc;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Agent task failed {TaskId}",
                    task.Id);
            }
        }

        if (tasks.Count > 0)
        {
            await db.SaveChangesAsync(
                cancellationToken);
        }
    }

    private async Task ExecuteTaskAsync(
        AgentTask task,
        IAgentService agentService,
        IAgentSessionStoreService sessionStore,
        IFcmService fcmService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting agent task {TaskId} {Name}",
            task.Id,
            task.Name);

        cancellationToken.ThrowIfCancellationRequested();

        var agent =
            agentService.GetAgent();

        // Każde zaplanowane zadanie posiada własną sesję.
        // Dzięki temu agent może zachować kontekst poprzednich uruchomień.
        var sessionId =
            $"agent-task-{task.Id}";

        await using var handle =
            await sessionStore.GetSessionAsync(
                sessionId,
                agent,
                cancellationToken);

        var prompt = $"""
            Wykonaj zaplanowane zadanie.

            Nazwa:
            {task.Name}

            Instrukcja:
            {task.Instruction}

            Jeżeli wykonywałeś to zadanie wcześniej,
            wykorzystaj poprzedni kontekst rozmowy.

            Wykonaj zadanie teraz.
            """;

        var result =
            await agent.RunAsync(
                message: prompt,
                session: handle.Session,
                options: null,
                cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Agent response for task {TaskId}: {Response}",
            task.Id,
            result.Text);

        cancellationToken.ThrowIfCancellationRequested();

        await SendNotificationAsync(
            task,
            result.Text ?? string.Empty,
            fcmService,
            cancellationToken);
    }

    private async Task SendNotificationAsync(
        AgentTask task,
        string response,
        IFcmService fcmService,
        CancellationToken cancellationToken)
    {
        try
        {
            var data =
                new Dictionary<string, string>
                {
                    ["taskId"] =
                        task.Id.ToString(),

                    ["taskName"] =
                        task.Name,

                    ["response"] =
                        response,

                    ["lastRunAt"] =
                        DateTimeOffset.UtcNow.ToString("O")
                };

            await fcmService.SendDataMessageToUserAsync(
                task.UserOid,
                data,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send FCM notification for task {TaskId}",
                task.Id);
        }
    }

    private static DateTime? CalculateNextRun(
        AgentTask task,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(
                task.CronExpression))
        {
            return null;
        }

        var cron =
            CronExpression.Parse(
                task.CronExpression);

        var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                task.TimeZoneId);

        return cron.GetNextOccurrence(
            nowUtc,
            timeZone);
    }
}