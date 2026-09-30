using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IMetaConfigRepository
{
    Task<MetaConfig?> GetActiveAsync(CancellationToken cancellationToken = default);
}
