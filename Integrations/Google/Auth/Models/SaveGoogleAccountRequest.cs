namespace Integrations.Google.Auth.Models
{
    public sealed record SaveGoogleAccountRequest(
    int UserId,
    string Email,
    string? RefreshToken,
    string? GrantedScopes);
}
