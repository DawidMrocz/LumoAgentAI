using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class EmailConfigRepository : IEmailConfigRepository
{
    private readonly AppDbContext _dbContext;

    public EmailConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmailConfig?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailConfigs.AsNoTracking().SingleOrDefaultAsync(x => x.IsActive, cancellationToken);
    }

    public async Task SaveAsync(EmailConfig config, CancellationToken cancellationToken = default)
    {
        if (config.Id == 0)
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;

            await _dbContext.EmailConfigs.AddAsync(config, cancellationToken);
        }
        else
        {
            config.UpdatedAt = DateTime.UtcNow;

            _dbContext.EmailConfigs.Update(config);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
