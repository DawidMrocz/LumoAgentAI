using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class GoogleAccountRepository : IGoogleAccountRepository
{
    private readonly AppDbContext _dbContext;

    public GoogleAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GoogleAccount?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.GoogleAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task SaveAsync(GoogleAccount googleAccount, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.GoogleAccounts.SingleOrDefaultAsync(x => x.UserId == googleAccount.UserId, cancellationToken);

        if (existing is null)
        {
            await _dbContext.GoogleAccounts.AddAsync(googleAccount, cancellationToken);
        }
        else
        {
            existing.Email = googleAccount.Email;
            existing.RefreshTokenProtected = googleAccount.RefreshTokenProtected;
            existing.GrantedScopes = googleAccount.GrantedScopes;
            existing.UpdatedAt = googleAccount.UpdatedAt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.GoogleAccounts.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (account is null)
            return;

        _dbContext.GoogleAccounts.Remove(account);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}