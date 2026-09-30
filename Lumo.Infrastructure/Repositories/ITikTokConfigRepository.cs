
using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories
{
    public interface ITikTokConfigRepository
    {
        Task<TikTokConfig?> GetActiveAsync(
            CancellationToken cancellationToken = default);
    }
}

