
namespace Lumo.Infrastructure.Entities
{
    public class TikTokConfig
    {
        public int Id { get; set; }

        public string ClientKey { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string RedirectUri { get; set; } = string.Empty;

        public string ApiBaseUrl { get; set; } =
            "https://open.tiktokapis.com";

        public string AuthBaseUrl { get; set; } =
            "https://www.tiktok.com";

        public string Scopes { get; set; } = string.Empty;

        public long MaxVideoSize { get; set; } = 287109120;

        public int MaxRetries { get; set; } = 3;

        public int TimeoutSeconds { get; set; } = 300;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
