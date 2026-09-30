
using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories
{
    public interface ITikTokAccountRepository
    {
        Task<TikTokAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task SaveAsync(
            TikTokAccount account,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}

