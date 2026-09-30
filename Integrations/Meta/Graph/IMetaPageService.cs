using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Meta.Graph
{
    public interface IMetaPageService
    {
        Task<IEnumerable<MetaPage>> GetUserPagesAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<MetaInstagramBusinessAccount?> GetInstagramBusinessAccountAsync(
            int userId,
            string pageId,
            CancellationToken cancellationToken = default);
    }
}
