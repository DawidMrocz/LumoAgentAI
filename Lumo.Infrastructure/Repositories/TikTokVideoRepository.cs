using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class TikTokVideoRepository : ITikTokVideoRepository
{
    private readonly AppDbContext _dbContext;

    public TikTokVideoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TikTokVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.TikTokVideos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<TikTokVideo?> GetByPublishIdAsync(string publishId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TikTokVideos.FirstOrDefaultAsync(x => x.PublishId == publishId, cancellationToken);
    }

    public Task<List<TikTokVideo>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return _dbContext.TikTokVideos
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<TikTokVideo>> GetPublishedByUserIdAsync(int userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TikTokVideos
            .Where(x => x.UserId == userId && x.Status == TikTokVideoStatus.Published);

        if (from.HasValue)
            query = query.Where(x => x.PublishedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.PublishedAt <= to.Value);

        return query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TikTokVideo video, CancellationToken cancellationToken = default)
    {
        await _dbContext.TikTokVideos.AddAsync(video, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TikTokVideo video, CancellationToken cancellationToken = default)
    {
        _dbContext.TikTokVideos.Update(video);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
