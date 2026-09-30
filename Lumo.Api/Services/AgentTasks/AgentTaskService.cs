using Cronos;
using Lumo.Api.Data.Agent;
using Lumo.Api.Data.User;
using Lumo.Api.DTO.AgentTask;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Api.Services.AgentTasks;

public class AgentTaskService : IAgentTaskService
{
    private const string DefaultTimeZoneId = "Europe/Warsaw";

    private readonly AppDbContext _db;

    public AgentTaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAgentTaskAsync(
        CreateAgentTaskToolRequest request,
        string userOid)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(userOid))
            throw new ArgumentException(
                "userOid jest wymagany.",
                nameof(userOid));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException(
                "Nazwa zadania jest wymagana.",
                nameof(request.Name));

        if (string.IsNullOrWhiteSpace(request.Instruction))
            throw new ArgumentException(
                "Instrukcja zadania jest wymagana.",
                nameof(request.Instruction));

        if (string.IsNullOrWhiteSpace(request.CronExpression) &&
            !request.ScheduledAt.HasValue)
        {
            throw new ArgumentException(
                "Zadanie musi mieć ScheduledAtUtc albo CronExpression.");
        }

        if (!string.IsNullOrWhiteSpace(request.CronExpression) &&
            request.ScheduledAt.HasValue)
        {
            throw new ArgumentException(
                "Zadanie nie może jednocześnie mieć ScheduledAtUtc i CronExpression.");
        }

        var entity = new AgentTask
        {
            Name = request.Name.Trim(),
            Instruction = request.Instruction.Trim(),
            UserOid = userOid.Trim(),

            ScheduledAt = request.ScheduledAt,
            CronExpression = request.CronExpression?.Trim(),

            TimeZoneId = DefaultTimeZoneId,
            Enabled = true
        };

        // TASK JEDNORAZOWY
        if (entity.ScheduledAt.HasValue)
        {
            entity.ScheduledAt = entity.ScheduledAt.Value;

            entity.NextRunAt = entity.ScheduledAt.Value;

        }

        // TASK CYKLICZNY
        else if (!string.IsNullOrWhiteSpace(entity.CronExpression))
        {
            entity.NextRunAt =
                CalculateNextRun(
                    entity.CronExpression,
                    entity.TimeZoneId);
        }

        _db.AgentTasks.Add(entity);

        await _db.SaveChangesAsync();
    }

    public async Task<List<AgentTaskResponse>> ListAgentTasksAsync(
        string userOid)
    {
        if (string.IsNullOrWhiteSpace(userOid))
            throw new ArgumentException(
                "userOid jest wymagany.",
                nameof(userOid));

        return await _db.AgentTasks
            .AsNoTracking()
            .Where(x => x.UserOid == userOid)
            .OrderBy(x => x.Id)
            .Select(x => new AgentTaskResponse
            {
                Id = x.Id,
                Name = x.Name,
                Instruction = x.Instruction,
                ScheduledAt = x.ScheduledAt,
                CronExpression = x.CronExpression,
                NextRunAt = x.NextRunAt,
                LastRunAt = x.LastRunAt,
                Enabled = x.Enabled,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task DeleteAgentTaskAsync(
        int id,
        string userOid)
    {
        if (id <= 0)
            throw new ArgumentException(
                "Nieprawidłowy identyfikator zadania.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(userOid))
            throw new ArgumentException(
                "userOid jest wymagany.",
                nameof(userOid));

        var task = await _db.AgentTasks
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserOid == userOid);

        if (task == null)
            throw new KeyNotFoundException(
                "AgentTask not found");

        _db.AgentTasks.Remove(task);

        await _db.SaveChangesAsync();
    }

    private static DateTime? CalculateNextRun(
        string cronExpression,
        string timeZoneId)
    {
        var cron = CronExpression.Parse(
            cronExpression);

        var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                timeZoneId);

        return cron.GetNextOccurrence(
            DateTime.Now,
            timeZone);
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            DateTimeKind.Unspecified =>
                throw new ArgumentException(
                    "ScheduledAtUtc musi być podany jako UTC."),

            _ =>
                throw new ArgumentException(
                    "Nieprawidłowy DateTime.")
        };
    }
}