using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.Data.Agent;

public class AgentTask
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserOid { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    public string Instruction { get; set; } = null!;

    // Dla taska jednorazowego
    public DateTime? ScheduledAt { get; set; }

    // Dla taska cyklicznego, np. "0 9 * * 5"
    [MaxLength(100)]
    public string? CronExpression { get; set; }

    // Strefa czasowa użytkownika, np. "Europe/Warsaw"
    [Required]
    [MaxLength(100)]
    public string TimeZoneId { get; set; } = "Europe/Warsaw";

    // Najbliższe konkretne uruchomienie w UTC.
    // Scheduler korzysta głównie z tego pola.
    public DateTime? NextRunAt { get; set; }

    // Ostatnie wykonanie taska.
    public DateTime? LastRunAt { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}