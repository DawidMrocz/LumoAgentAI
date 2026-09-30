using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IMetaAccountRepository
{
    Task<MetaAccount?> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task SaveAsync(MetaAccount account, CancellationToken cancellationToken = default);

    Task DeleteAsync(int userId, CancellationToken cancellationToken = default);
}
