using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Auth
{
    public interface ITikTokCredentialProvider
    {
        Task<string> GetAccessTokenAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
