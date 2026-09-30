using System;

namespace Lumo.Api.Data.User
{
    public class GmailOAuthState
    {
        public int Id { get; set; }
        public string State { get; set; } = null!;
        public string UserOid { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
