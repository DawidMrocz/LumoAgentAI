namespace Lumo.Api.Data.User
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTimeOffset? ModifyDate { get; set; }
        public int? ModifyUserId { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; }

        public bool Used { get; set; }

        public DateTime ExpiresAt { get; set; }

        public User User { get; set; }
    }
}
