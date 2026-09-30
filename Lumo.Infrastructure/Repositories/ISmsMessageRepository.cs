using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface ISmsMessageRepository
{
    Task<SmsMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SmsMessage?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    Task<List<SmsMessage>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken cancellationToken = default);

    Task<List<SmsMessage>> GetInRangeAsync(int? userId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);

    Task AddAsync(SmsMessage message, CancellationToken cancellationToken = default);

    Task UpdateAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
