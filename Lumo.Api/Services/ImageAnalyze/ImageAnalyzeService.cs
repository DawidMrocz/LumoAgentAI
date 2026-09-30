#pragma warning disable OPENAI001

using OpenAI.Responses;

namespace Lumo.Api.Services.ImageAnalyze
{
    public class ImageAnalyzeService : IImageAnalyzeService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ImageAnalyzeService(
            IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeAsync(
            string attachmentId,
            string question)
        {
            if (!Guid.TryParse(attachmentId, out var fileId))
            {
                return "Nieprawidłowy identyfikator pliku.";
            }

            // ============================================
            // 1. Katalog uploadów
            // ============================================

            var uploadsPath =
                Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data",
                    "uploads"
                );

            if (!Directory.Exists(uploadsPath))
                return "Nie znaleziono katalogu plików.";

            // ============================================
            // 2. Znajdź plik po GUID
            // ============================================

            var files =
                Directory.GetFiles(
                    uploadsPath,
                    $"{fileId:N}.*"
                );

            if (files.Length == 0)
            {
                return "Nie znaleziono załączonego obrazu.";
            }

            var filePath = files[0];

            // ============================================
            // 3. Sprawdź rozszerzenie
            // ============================================

            var extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();

            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => null
            };

            if (contentType == null)
            {
                return "Załącznik nie jest obsługiwanym obrazem.";
            }

            // ============================================
            // 4. API KEY
            // ============================================

            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Brak OPENAI_API_KEY."
                );
            }

            // ============================================
            // 5. Wczytaj obraz
            // ============================================

            var imageBytes =
                await File.ReadAllBytesAsync(filePath);

            var imageData =
                BinaryData.FromBytes(imageBytes);

            var base64 = Convert.ToBase64String(imageBytes);
            var imageUri = new Uri($"data:{contentType};base64,{base64}");

            var imagePart =
                ResponseContentPart.CreateInputImagePart(
                    imageUri,
                    ResponseImageDetailLevel.High
                );

            var client =
                new ResponsesClient(apiKey);

            //var responseResult =
            //    await client.CreateResponseAsync(
            //        "gpt-5.6",
            //        [
            //            ResponseItem.CreateUserMessageItem(
            //                [
            //                    ResponseContentPart.CreateInputTextPart(
            //                        question
            //                    ),

            //                    ResponseContentPart.CreateInputImagePart(
            //                        imageData,
            //                        contentType
            //                    )
            //                ]
            //            )
            //        ]
            //    );

            var responseResult =
    await client.CreateResponseAsync(
        "gpt-5.6",
        [
            ResponseItem.CreateUserMessageItem(
                [
                    ResponseContentPart.CreateInputTextPart(question),
                    imagePart
                ]
            )
        ]
    );

            var response = responseResult.Value;
            return response.GetOutputText();
        }
    }
}