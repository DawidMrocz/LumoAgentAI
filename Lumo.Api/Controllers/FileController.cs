using Lumo.Api.ApiModels.Chat;
using Lumo.Api.Services.FileSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;
        private readonly IFileSearchService _fileSearchService;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".gif",
                ".pdf",
                ".txt",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx"
            };

        private const long MaxFileSize =
            25L * 1024L * 1024L;

        public FileController(
            IWebHostEnvironment environment,
            ILogger<FileController> logger,
            IFileSearchService fileSearchService)
        {
            _environment = environment;
            _logger = logger;
            _fileSearchService = fileSearchService;
        }


        // =====================================================
        // POST /api/files/upload
        //
        // Zapisuje plik lokalnie.
        // =====================================================

        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult<FileUploadResponse>> Upload(
            [FromForm] IFormFile? file)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano pliku."
                    });
                }

                if (file.Length <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Plik jest pusty."
                    });
                }

                if (file.Length > MaxFileSize)
                {
                    return BadRequest(new
                    {
                        message =
                            "Plik jest zbyt duży. Maksymalny rozmiar to 25 MB."
                    });
                }

                var originalFileName =
                    Path.GetFileName(file.FileName);

                var extension =
                    Path.GetExtension(originalFileName)
                        .ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension) ||
                    !AllowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message =
                            "Ten typ pliku nie jest obsługiwany."
                    });
                }

                var uploadsPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "App_Data",
                        "uploads"
                    );

                Directory.CreateDirectory(uploadsPath);

                var fileId =
                    Guid.NewGuid();

                var safeFileName =
                    $"{fileId:N}{extension}";

                var filePath =
                    Path.Combine(
                        uploadsPath,
                        safeFileName
                    );

                await using (
                    var stream =
                        new FileStream(
                            filePath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            64 * 1024,
                            useAsync: true))
                {
                    await file.CopyToAsync(stream);
                }

                var contentType =
                    GetContentType(
                        extension,
                        file.ContentType
                    );

                var downloadUrl =
                    $"{Request.Scheme}://{Request.Host}/api/files/{fileId}";

                return Ok(
                    new FileUploadResponse
                    {
                        Id = fileId.ToString(),
                        Url = downloadUrl,
                        FileName = originalFileName,
                        ContentType = contentType,
                        Size = file.Length
                    }
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Błąd podczas uploadu pliku."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Nie udało się zapisać pliku."
                    }
                );
            }
        }


        // =====================================================
        // POST /api/files/vector-store
        //
        // Upload lokalnego pliku do OpenAI Files API
        // + tworzenie Vector Store
        // + dodanie pliku do Vector Store.
        // =====================================================

        [HttpPost("vector-store")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult<VectorStoreResponse>>
            CreateVectorStore(
                [FromForm] UploadDocumentRequest request)
        {
            try
            {
                if (request.File == null)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano pliku."
                    });
                }

                if (request.File.Length <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Plik jest pusty."
                    });
                }

                if (request.File.Length > MaxFileSize)
                {
                    return BadRequest(new
                    {
                        message =
                            "Plik jest zbyt duży. Maksymalny rozmiar to 25 MB."
                    });
                }

                var originalFileName =
                    Path.GetFileName(request.File.FileName);

                var extension =
                    Path.GetExtension(originalFileName)
                        .ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension) ||
                    !AllowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message =
                            "Ten typ pliku nie jest obsługiwany."
                    });
                }

                // ---------------------------------------------
                // TEMP DIRECTORY
                // ---------------------------------------------

                var tempPath =
                    Path.Combine(
                        Path.GetTempPath(),
                        "lumo",
                        "files"
                    );

                Directory.CreateDirectory(tempPath);

                var tempFileName =
                    $"{Guid.NewGuid():N}{extension}";

                var tempFilePath =
                    Path.Combine(
                        tempPath,
                        tempFileName
                    );

                // ---------------------------------------------
                // SAVE TEMP FILE
                // ---------------------------------------------

                await using (
                    var stream =
                        new FileStream(
                            tempFilePath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            64 * 1024,
                            useAsync: true))
                {
                    await request.File.CopyToAsync(stream);
                }

                try
                {
                    // -----------------------------------------
                    // OPENAI FILE + VECTOR STORE
                    // -----------------------------------------

                    var vectorStoreId =
                        await _fileSearchService.AddFileAsync(
                            tempFilePath,
                            originalFileName
                        );

                    return Ok(
                        new VectorStoreResponse
                        {
                            VectorStoreId =
                                vectorStoreId,

                            FileName =
                                originalFileName
                        }
                    );
                }
                finally
                {
                    // -----------------------------------------
                    // DELETE TEMP FILE
                    // -----------------------------------------

                    try
                    {
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(
                                tempFilePath
                            );
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(
                            exception,
                            "Nie udało się usunąć pliku tymczasowego {FilePath}",
                            tempFilePath
                        );
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Błąd tworzenia Vector Store."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Nie udało się utworzyć Vector Store.",
                        detail =
                            exception.Message
                    }
                );
            }
        }


        // =====================================================
        // GET /api/files/search
        //
        // Wyszukiwanie w Vector Store.
        //
        // /api/files/search?vectorStoreId=vs_xxx&query=...
        // =====================================================

        [HttpGet("search")]
        public async Task<ActionResult<VectorStoreSearchResponse>>
            Search(
                [FromQuery] string vectorStoreId,
                [FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                {
                    return BadRequest(new
                    {
                        message =
                            "vectorStoreId jest wymagane."
                    });
                }

                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new
                    {
                        message =
                            "query jest wymagane."
                    });
                }

                var result =
                    await _fileSearchService.SearchAsync(
                        vectorStoreId,
                        query
                    );

                return Ok(
                    new VectorStoreSearchResponse
                    {
                        VectorStoreId =
                            vectorStoreId,

                        Query =
                            query,

                        Result =
                            result
                    }
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Błąd wyszukiwania w Vector Store."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Nie udało się wykonać wyszukiwania.",
                        detail =
                            exception.Message
                    }
                );
            }
        }


        // =====================================================
        // GET /api/files/{id}
        // =====================================================

        [HttpGet("{id:guid}")]
        public IActionResult Download(
            Guid id)
        {
            try
            {
                var uploadsPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "App_Data",
                        "uploads"
                    );

                if (!Directory.Exists(uploadsPath))
                {
                    return NotFound();
                }

                var files =
                    Directory.GetFiles(
                        uploadsPath,
                        $"{id:N}.*"
                    );

                if (files.Length == 0)
                {
                    return NotFound();
                }

                var filePath =
                    files[0];

                var extension =
                    Path.GetExtension(filePath)
                        .ToLowerInvariant();

                var contentType =
                    GetContentType(
                        extension,
                        null
                    );

                var fileName =
                    Path.GetFileName(filePath);

                return PhysicalFile(
                    filePath,
                    contentType,
                    fileName,
                    enableRangeProcessing: true
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Błąd pobierania pliku {FileId}",
                    id
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Nie udało się pobrać pliku."
                    }
                );
            }
        }


        // =====================================================
        // CONTENT TYPE
        // =====================================================

        private static string GetContentType(
            string extension,
            string? uploadedContentType)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" =>
                    "image/jpeg",

                ".png" =>
                    "image/png",

                ".webp" =>
                    "image/webp",

                ".gif" =>
                    "image/gif",

                ".pdf" =>
                    "application/pdf",

                ".txt" =>
                    "text/plain",

                ".doc" =>
                    "application/msword",

                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".xls" =>
                    "application/vnd.ms-excel",

                ".xlsx" =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                _ =>
                    string.IsNullOrWhiteSpace(uploadedContentType)
                        ? "application/octet-stream"
                        : uploadedContentType
            };
        }
    }


    // =====================================================
    // RESPONSES
    // =====================================================

    public class FileUploadResponse
    {
        public string Id { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }
    }


    public class VectorStoreResponse
    {
        public string VectorStoreId { get; set; } =
            string.Empty;

        public string FileName { get; set; } =
            string.Empty;
    }


    public class VectorStoreSearchResponse
    {
        public string VectorStoreId { get; set; } =
            string.Empty;

        public string Query { get; set; } =
            string.Empty;

        public string Result { get; set; } =
            string.Empty;
    }
}