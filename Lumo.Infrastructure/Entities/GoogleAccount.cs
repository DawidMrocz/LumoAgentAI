public sealed class GoogleAccount
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string RefreshTokenProtected { get; set; } = string.Empty;

    public string GrantedScopes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}