using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class VeoVideoRepository : IVeoVideoRepository
{
    private readonly AppDbContext _dbContext;

    public VeoVideoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<VeoVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.VeoVideos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<VeoVideo?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return _dbContext.VeoVideos.FirstOrDefaultAsync(x => x.JobId == jobId, cancellationToken);
    }

    public Task<List<VeoVideo>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return _dbContext.VeoVideos
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<VeoVideo>> GetByUserIdAsync(int userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.VeoVideos.Where(x => x.UserId == userId);

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value);

        return query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(VeoVideo video, CancellationToken cancellationToken = default)
    {
        await _dbContext.VeoVideos.AddAsync(video, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(VeoVideo video, CancellationToken cancellationToken = default)
    {
        _dbContext.VeoVideos.Update(video);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
