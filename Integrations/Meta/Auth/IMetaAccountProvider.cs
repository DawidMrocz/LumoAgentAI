using Integrations.Meta.Auth.Models;
using Lumo.Infrastructure.Entities;

namespace Integrations.Meta.Auth
{
    public interface IMetaAccountProvider
    {
        Task<MetaAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task SaveAsync(
            SaveMetaAccountRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
