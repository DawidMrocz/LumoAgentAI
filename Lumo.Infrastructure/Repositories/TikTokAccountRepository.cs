using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories
{
    internal sealed class TikTokAccountRepository : ITikTokAccountRepository
    {
        private readonly AppDbContext _dbContext;

        public TikTokAccountRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TikTokAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.TikTokAccounts
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);
        }

        public async Task SaveAsync(
            TikTokAccount account,
            CancellationToken cancellationToken = default)
        {
            if (account.Id == 0)
            {
                _dbContext.TikTokAccounts.Add(account);
            }
            else
            {
                _dbContext.TikTokAccounts.Update(account);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var account = await _dbContext.TikTokAccounts
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            if (account is null)
                return;

            _dbContext.TikTokAccounts.Remove(account);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
