using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IVeoVideoRepository
{
    Task<VeoVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<VeoVideo?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default);

    Task<List<VeoVideo>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default);

    Task<List<VeoVideo>> GetByUserIdAsync(int userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);

    Task AddAsync(VeoVideo video, CancellationToken cancellationToken = default);

    Task UpdateAsync(VeoVideo video, CancellationToken cancellationToken = default);
}
