
using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories
{
    internal sealed class TikTokConfigRepository : ITikTokConfigRepository
    {
        private readonly AppDbContext _dbContext;

        public TikTokConfigRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TikTokConfig?> GetActiveAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.TikTokConfigs
                .FirstOrDefaultAsync(
                    x => x.IsActive,
                    cancellationToken);
        }
    }
}

