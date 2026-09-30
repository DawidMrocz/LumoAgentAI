namespace Lumo.Api.DTO.User
{
    public class SessionResponse
    {
        public int Id { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUsedAt { get; set; }

        public DateTime AbsoluteExpiresAt { get; set; }

        /// <summary>
        /// Czy to sesja, z której wykonano bieżące żądanie.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
