namespace Integrations.Meta.Auth
{
    public interface IMetaCredentialProvider
    {
        Task<string> GetAccessTokenAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
