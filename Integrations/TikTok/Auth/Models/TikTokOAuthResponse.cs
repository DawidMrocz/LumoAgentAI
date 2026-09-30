using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Auth.Models
{
    public sealed class TikTokOAuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public string OpenId { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public int RefreshExpiresIn { get; set; }

        public string Scope { get; set; } = string.Empty;
    }
}
