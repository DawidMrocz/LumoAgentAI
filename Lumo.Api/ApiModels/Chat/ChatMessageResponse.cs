namespace Lumo.Api.ApiModels.Chat
{
    public class ChatMessageResponse
    {
        public string Id { get; set; } = string.Empty;

        public string? Text { get; set; }

        public bool FromUser { get; set; }

        public ChatAttachmentResponse? Attachment { get; set; }
    }

    public class ChatAttachmentResponse
    {
        public string Id { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }
    }
}
