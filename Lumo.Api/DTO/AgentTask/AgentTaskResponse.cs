using Lumo.Api.Attributes.Validation;

namespace Lumo.Api.DTO.AgentTask
{
    [TrimAllStrings]
    public class AgentTaskResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Instruction { get; set; } = null!;
        public DateTime? ScheduledAt { get; set; }
        public string? CronExpression { get; set; }
        public DateTime? NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
