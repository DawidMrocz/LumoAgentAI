using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IEmailConfigRepository
{
    Task<EmailConfig?> GetActiveAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(EmailConfig config, CancellationToken cancellationToken = default);
}
