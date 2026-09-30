using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class VeoConfigRepository : IVeoConfigRepository
{
    private readonly AppDbContext _dbContext;

    public VeoConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<VeoConfig?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.VeoConfigs.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
    }
}
