namespace Integrations.Google.Auth
{
    public interface IGoogleOAuthService
    {
        Task<string> CreateAuthorizationUrlAsync(int userId, CancellationToken cancellationToken = default);
        Task HandleCallbackAsync(string code, string state, CancellationToken cancellationToken = default);
    }
}
