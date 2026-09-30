namespace Integrations.Meta.Auth
{
    public interface IMetaOAuthService
    {
        Task<string> CreateAuthorizationUrlAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task HandleCallbackAsync(
            string code,
            string state,
            CancellationToken cancellationToken = default);
    }
}
