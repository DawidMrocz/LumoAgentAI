using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IEmailMessageRepository
{
    Task<EmailMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<EmailMessage>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default);

    Task<List<EmailMessage>> GetInRangeAsync(int? userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);

    Task AddAsync(EmailMessage message, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
