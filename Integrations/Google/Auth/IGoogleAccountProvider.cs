using Integrations.Google.Auth.Models;

namespace Integrations.Google.Auth
{
    internal interface IGoogleAccountProvider
    {
        Task<GoogleAccount?> GetAsync(int userId, CancellationToken cancellationToken = default);
        Task SaveAsync(SaveGoogleAccountRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int userId,CancellationToken cancellationToken = default);
    }
}
