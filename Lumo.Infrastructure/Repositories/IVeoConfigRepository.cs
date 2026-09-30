using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IVeoConfigRepository
{
    Task<VeoConfig?> GetActiveAsync(CancellationToken cancellationToken = default);
}
