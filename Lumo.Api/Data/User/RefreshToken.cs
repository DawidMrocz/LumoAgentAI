namespace Lumo.Api.Data.User
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTimeOffset? ModifyDate { get; set; }
        public int? ModifyUserId { get; set; }
        public bool IsActive { get; set; } = true;
        public string RefreshTokenHash { get; set; } = null!;
        public string? IpAddress { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public int? ReplacedByTokenId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime AbsoluteExpiresAt { get; set; }
        public DateTime LastUsedAt { get; set; }
    }
}
