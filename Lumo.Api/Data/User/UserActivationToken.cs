namespace Lumo.Api.Data.User
{
    public class UserActivationToken
    {
        public int Id { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTimeOffset? ModifyDate { get; set; }
        public int? ModifyUserId { get; set; }
        public bool IsActive { get; set; } = true;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
    }
}
