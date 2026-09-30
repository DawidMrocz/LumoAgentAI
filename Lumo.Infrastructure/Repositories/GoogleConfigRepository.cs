using Lumo.Infrastructure;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class GoogleConfigRepository : IGoogleConfigRepository
{
    private readonly AppDbContext _dbContext;

    public GoogleConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GoogleConfig?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.GoogleConfigs.AsNoTracking().SingleOrDefaultAsync(x => x.IsActive, cancellationToken);
    }
}