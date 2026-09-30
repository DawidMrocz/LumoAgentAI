using System;
namespace Lumo.Api.Data.User
{
    public class GmailCredential
    {
        public int Id { get; set; }
        public string UserOid { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
