using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class EmailMessageRepository : IEmailMessageRepository
{
    private readonly AppDbContext _dbContext;

    public EmailMessageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmailMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<EmailMessage>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailMessages
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<EmailMessage>> GetInRangeAsync(int? userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EmailMessages.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        return query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
