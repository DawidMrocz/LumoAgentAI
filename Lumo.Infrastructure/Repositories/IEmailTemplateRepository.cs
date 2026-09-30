using Lumo.Infrastructure.Entities;

namespace Lumo.Infrastructure.Repositories;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<List<EmailTemplate>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default);

    Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
