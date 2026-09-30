using Google.Apis.Auth.OAuth2;

namespace Integrations.Google.Auth
{
    public interface IGoogleCredentialProvider
    {
        Task<UserCredential> GetCredentialAsync(int userId, CancellationToken cancellationToken = default);
    }
}
