using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface ITikTokVideoRepository
{
    Task<TikTokVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TikTokVideo?> GetByPublishIdAsync(string publishId, CancellationToken cancellationToken = default);

    Task<List<TikTokVideo>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default);

    Task<List<TikTokVideo>> GetPublishedByUserIdAsync(int userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);

    Task AddAsync(TikTokVideo video, CancellationToken cancellationToken = default);

    Task UpdateAsync(TikTokVideo video, CancellationToken cancellationToken = default);
}
