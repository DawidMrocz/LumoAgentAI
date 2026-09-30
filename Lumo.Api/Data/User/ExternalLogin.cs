namespace Lumo.Api.Data.User
{
    public class ExternalLogin
    {
        public int Id { get; set; }
        public string Provider { get; set; } = null!; // "Google"
        public string ProviderUserId { get; set; } = null!; // payload.Subject
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTimeOffset CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTimeOffset? ModifyDate { get; set; }
        public int? ModifyUserId { get; set; }
    }
}
