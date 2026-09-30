namespace Lumo.Infrastructure.Entities
{
    public sealed class MetaAccount
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? MetaUserId { get; set; }

        public string? Email { get; set; }

        public string AccessTokenProtected { get; set; } = string.Empty;

        public string GrantedScopes { get; set; } = string.Empty;

        public DateTime? TokenExpiresAtUtc { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
