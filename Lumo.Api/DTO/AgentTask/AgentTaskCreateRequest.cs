namespace Lumo.Api.DTO.AgentTask
{
    public sealed class CreateAgentTaskToolRequest
    {
        public string Name { get; set; } = null!;
        public string Instruction { get; set; } = null!;
        public DateTime? ScheduledAt { get; set; }
        public string? CronExpression { get; set; }
    }
}
