namespace Lumo.Infrastructure.Entities
{
    public sealed class TikTokAccount
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string OpenId { get; set; } = string.Empty;

        public string AccessTokenProtected { get; set; } = string.Empty;

        public string RefreshTokenProtected { get; set; } = string.Empty;

        public string GrantedScopes { get; set; } = string.Empty;

        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }
}
