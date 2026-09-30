namespace Lumo.Infrastructure.Repositories;

public interface IGoogleAccountRepository
{
    Task<GoogleAccount?> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task SaveAsync(GoogleAccount googleAccount, CancellationToken cancellationToken = default);

    Task DeleteAsync(int userId, CancellationToken cancellationToken = default);
}