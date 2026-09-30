using Integrations.TikTok.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;

namespace Integrations.TikTok.Auth
{
    public interface ITikTokAccountProvider
    {
        Task<TikTokAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task SaveAsync(
            SaveTikTokAccountRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
