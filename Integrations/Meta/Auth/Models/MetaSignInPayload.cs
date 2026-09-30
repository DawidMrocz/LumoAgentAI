namespace Integrations.Meta.Auth.Models
{
    public sealed record MetaSignInPayload(
        string Subject,
        string Email,
        string? FirstName,
        string? LastName);
}
