using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class SmsConfigRepository : ISmsConfigRepository
{
    private readonly AppDbContext _dbContext;

    public SmsConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SmsConfig?> GetAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SmsConfigs.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(SmsConfig config, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.SmsConfigs.FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            _dbContext.SmsConfigs.Add(config);
        }
        else
        {
            existing.AccountSid = config.AccountSid;
            existing.AuthToken = config.AuthToken;
            existing.PhoneNumber = config.PhoneNumber;
            existing.TestMode = config.TestMode;
            existing.MaxRetries = config.MaxRetries;
            existing.TimeoutSeconds = config.TimeoutSeconds;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
