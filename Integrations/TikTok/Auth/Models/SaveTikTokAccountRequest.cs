using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Auth.Models
{
    public sealed record SaveTikTokAccountRequest(
    int UserId,
    string OpenId,
    string AccessToken,
    string RefreshToken,
    string GrantedScopes,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
}
