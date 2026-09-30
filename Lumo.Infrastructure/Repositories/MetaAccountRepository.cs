using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class MetaAccountRepository : IMetaAccountRepository
{
    private readonly AppDbContext _dbContext;

    public MetaAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MetaAccount?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MetaAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task SaveAsync(MetaAccount account, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.MetaAccounts.SingleOrDefaultAsync(x => x.UserId == account.UserId, cancellationToken);

        if (existing is null)
        {
            await _dbContext.MetaAccounts.AddAsync(account, cancellationToken);
        }
        else
        {
            existing.MetaUserId = account.MetaUserId;
            existing.Email = account.Email;
            existing.AccessTokenProtected = account.AccessTokenProtected;
            existing.GrantedScopes = account.GrantedScopes;
            existing.TokenExpiresAtUtc = account.TokenExpiresAtUtc;
            existing.UpdatedAt = account.UpdatedAt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.MetaAccounts.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (account is null)
            return;

        _dbContext.MetaAccounts.Remove(account);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
