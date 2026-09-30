using Integrations.TikTok.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Factory
{
    public interface ITikTokServiceFactory
    {
        Task<ITikTokVideoService> CreateVideoServiceAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
