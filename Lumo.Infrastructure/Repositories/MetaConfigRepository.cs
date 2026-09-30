using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class MetaConfigRepository : IMetaConfigRepository
{
    private readonly AppDbContext _dbContext;

    public MetaConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MetaConfig?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.MetaConfigs.AsNoTracking().SingleOrDefaultAsync(x => x.IsActive, cancellationToken);
    }
}
