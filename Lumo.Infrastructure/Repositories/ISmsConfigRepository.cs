using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface ISmsConfigRepository
{
    Task<SmsConfig?> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(SmsConfig config, CancellationToken cancellationToken = default);
}
