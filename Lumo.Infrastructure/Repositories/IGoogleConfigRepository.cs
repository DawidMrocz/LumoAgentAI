using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IGoogleConfigRepository
{
    Task<GoogleConfig?> GetActiveAsync(CancellationToken cancellationToken = default);
}