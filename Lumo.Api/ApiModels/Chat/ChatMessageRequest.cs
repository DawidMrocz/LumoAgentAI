using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.Chat
{
    public class ChatMessageRequest
    {
        /// <summary>
        /// Treść pytania lub polecenia wysłanego do asystenta AI.
        /// </summary>
        [Required(ErrorMessage = "Prompt jest wymagany.")]
        [MinLength(1, ErrorMessage = "Prompt nie może być pusty.")]
        [MaxLength(10_000, ErrorMessage = "Prompt może mieć maksymalnie 10 000 znaków.")]
        public string Text { get; set; } = string.Empty;

        public ChatAttachmentRequest? Attachment { get; set; }
    }

    public class ChatAttachmentRequest
    {
        [Required(ErrorMessage = "Identyfikator załącznika jest wymagany.")]
        [MaxLength(200, ErrorMessage = "Identyfikator załącznika jest zbyt długi.")]
        public string Id { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Adres URL załącznika jest zbyt długi.")]
        public string Url { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa pliku jest wymagana.")]
        [MaxLength(260, ErrorMessage = "Nazwa pliku jest zbyt długa.")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content-Type załącznika jest wymagany.")]
        [MaxLength(150, ErrorMessage = "Content-Type jest zbyt długi.")]
        [RegularExpression(@"^[a-zA-Z0-9!#$&\-\^_.+]+/[a-zA-Z0-9!#$&\-\^_.+]+$",
            ErrorMessage = "Niepoprawny format Content-Type.")]
        public string ContentType { get; set; } = string.Empty;

        [Range(0, 100L * 1024 * 1024, ErrorMessage = "Rozmiar załącznika jest poza dozwolonym zakresem.")]
        public long Size { get; set; }
    }
}
