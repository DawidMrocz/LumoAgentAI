namespace Integrations.Meta.Auth.Models
{
    public sealed record SaveMetaAccountRequest(
    int UserId,
    string? MetaUserId,
    string? Email,
    string AccessToken,
    string? GrantedScopes,
    DateTime? TokenExpiresAtUtc);
}
