#pragma warning disable OPENAI001

using OpenAI.Files;
using OpenAI.VectorStores;
using System.ClientModel;
using System.Text.Json;

namespace Lumo.Api.Services.FileSearch
{
    public class FileSearchService : IFileSearchService
    {
        private readonly OpenAIFileClient _fileClient;
        private readonly VectorStoreClient _vectorStoreClient;
        private readonly ILogger<FileSearchService> _logger;

        public FileSearchService(IConfiguration configuration, ILogger<FileSearchService> logger)
        {
            var apiKey =
                configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "Brak OpenAI API Key."
                );

            _fileClient = new OpenAIFileClient(apiKey);
            _vectorStoreClient = new VectorStoreClient(apiKey);
            _logger = logger;
        }

        // ============================================================
        // UPLOAD + VECTOR STORE
        // ============================================================

        public async Task<string> AddFileAsync(
            string filePath,
            string fileName)
        {
            // 1. Upload pliku do Files API
            await using var stream = File.OpenRead(filePath);

            var fileResult =
     await _fileClient.UploadFileAsync(
         stream,
         fileName,
         FileUploadPurpose.UserData
     );

            var file = fileResult.Value;

            // 2. Utworzenie Vector Store
            var createOptions = new VectorStoreCreationOptions
            {
                Name = $"Lumo - {fileName}"
            };

            using var createContent =
                BinaryContent.Create(createOptions);

            try
            {
                var vectorStoreResult =
     await _vectorStoreClient.CreateVectorStoreAsync(
         createContent
     );

                var rawResponse =
                    vectorStoreResult.GetRawResponse();

                var json =
                    rawResponse.Content.ToString();

                var vectorStore =
       JsonSerializer.Deserialize<VectorStoreDto>(
           json,
           new JsonSerializerOptions
           {
               PropertyNameCaseInsensitive = true
           }
       );

           

                if (vectorStore?.Id == null)
                {
                    throw new InvalidOperationException(
                        "Nie udało się utworzyć Vector Store."
                    );
                }

                // 3. Dodanie pliku do Vector Store
                var addFileRequest = BinaryContent.Create(
                     BinaryData.FromObjectAsJson(
                         new
                         {
                             file_id = file.Id
                         }
                     )
                 );

                await _vectorStoreClient.AddFileToVectorStoreAsync(
                    vectorStore.Id,
                    addFileRequest
                );

                // 4. Zwracamy ID Vector Store
                return vectorStore.Id;
            }
            catch (ClientResultException exception)
            {
                _logger.LogError(
                    exception,
                    "Błąd OpenAI podczas tworzenia Vector Store. " +
                    "StatusCode: {StatusCode}, Message: {Message}",
                    exception.Status,
                    exception.Message
                );

                throw new InvalidOperationException(
                    $"Nie udało się utworzyć Vector Store w OpenAI. " +
                    $"HTTP {exception.Status}: {exception.Message}",
                    exception
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Nieoczekiwany błąd podczas tworzenia Vector Store."
                );

                throw;
            }
        }

        // ============================================================
        // SEARCH
        // ============================================================

        public async Task<string> SearchAsync(
            string vectorStoreId,
            string query)
        {
            if (string.IsNullOrWhiteSpace(vectorStoreId))
                throw new ArgumentException(
                    "Vector Store ID jest wymagane.",
                    nameof(vectorStoreId)
                );

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException(
                    "Query jest wymagane.",
                    nameof(query)
                );

            // Request do:
            //
            // POST /vector_stores/{vector_store_id}/search
            //
            // {
            //     "query": "..."
            // }

            var searchRequest = BinaryContent.Create(
                BinaryData.FromObjectAsJson(
                    new
                    {
                        query = query,
                        max_num_results = 10
                    }
                )
            );

            var result =
                await _vectorStoreClient.SearchVectorStoreAsync(
                    vectorStoreId,
                    searchRequest
                );

            var json =
                result
                    .GetRawResponse()
                    .Content
                    .ToString();

            var searchResponse =
                JsonSerializer.Deserialize<VectorStoreSearchResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            if (searchResponse?.Data == null ||
                searchResponse.Data.Count == 0)
            {
                return "Nie znaleziono informacji w dokumentach.";
            }

            // Zwracamy znalezione fragmenty tekstu.
            var text = string.Join(
                "\n\n---\n\n",
                searchResponse.Data
                    .SelectMany(x => x.Content ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                    .Select(x => x.Text)
            );

            return string.IsNullOrWhiteSpace(text)
                ? "Nie znaleziono informacji w dokumentach."
                : text;
        }

        // ============================================================
        // DTO
        // ============================================================

        private sealed class VectorStoreDto
        {
            public string? Id { get; set; }
            public string? Object { get; set; }
            public long CreatedAt { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public long UsageBytes { get; set; }
            public VectorStoreFileCountsDto? FileCounts { get; set; }
            public string? Status { get; set; }
            public VectorStoreExpiresAfterDto? ExpiresAfter { get; set; }
            public long? ExpiresAt { get; set; }
            public long? LastActiveAt { get; set; }
            public Dictionary<string, string>? Metadata { get; set; }
        }

        private sealed class VectorStoreFileCountsDto
        {
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public int Cancelled { get; set; }
            public int Total { get; set; }
        }

        private sealed class VectorStoreExpiresAfterDto
        {
            public string? Anchor { get; set; }
            public int Days { get; set; }
        }

        private sealed class VectorStoreSearchResponse
        {
            public List<VectorStoreSearchResult> Data { get; set; }
        }

        private sealed class VectorStoreSearchResult
        {
            public string FileId { get; set; }

            public string Filename { get; set; }

            public List<VectorStoreSearchContent> Content { get; set; }
        }

        private sealed class VectorStoreSearchContent
        {
            public string Type { get; set; }

            public string Text { get; set; }
        }
    }
}