using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure.Repositories;

internal sealed class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly AppDbContext _dbContext;

    public EmailTemplateRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailTemplates.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public Task<List<EmailTemplate>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EmailTemplates.AsQueryable();

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        return query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        template.CreatedAt = now;
        template.UpdatedAt = now;

        await _dbContext.EmailTemplates.AddAsync(template, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        template.UpdatedAt = DateTime.UtcNow;

        _dbContext.EmailTemplates.Update(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (template is null)
        {
            return;
        }

        _dbContext.EmailTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
