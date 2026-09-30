namespace Lumo.Api.DTO.RAG
{
    public sealed class VectorPoint
    {
        public ulong Id { get; set; }
        public float[] Vector { get; set; } = [];
        public Dictionary<string, object?> Payload { get; set; } = [];

        public string UserId { get; set; } = string.Empty;
    }
}
