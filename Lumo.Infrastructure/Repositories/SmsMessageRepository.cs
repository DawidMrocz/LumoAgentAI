using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class SmsMessageRepository : ISmsMessageRepository
{
    private readonly AppDbContext _dbContext;

    public SmsMessageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SmsMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.SmsMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<SmsMessage?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return _dbContext.SmsMessages.FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);
    }

    public Task<List<SmsMessage>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return _dbContext.SmsMessages
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<SmsMessage>> GetInRangeAsync(int? userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SmsMessages.AsQueryable();

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value);

        return query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.SmsMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        _dbContext.SmsMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
