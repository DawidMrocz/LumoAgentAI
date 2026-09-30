namespace Lumo.Api.ApiModels.Chat
{
    public class UploadDocumentRequest
    {
        /// <summary>
        /// Plik do uploadowania
        /// </summary>
        public IFormFile File { get; set; } = default!;
    }
}
