using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Auth
{
    public interface ITikTokOAuthService
    {
        Task<string> CreateAuthorizationUrlAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task HandleCallbackAsync(
            string code,
            string state,
            CancellationToken cancellationToken = default);
    }
}
