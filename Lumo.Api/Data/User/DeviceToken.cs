using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.Data.User;

public class DeviceToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserOid { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
