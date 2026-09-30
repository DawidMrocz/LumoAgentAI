namespace Integrations.Google.Auth.Models
{
    public sealed record GoogleSignInPayload(
        string Subject,
        string Email,
        string? FirstName,
        string? LastName);
}
