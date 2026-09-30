using Lumo.Api.ApiModels.Chat;
using Lumo.Api.DTO.Chat;
using Lumo.Api.Services.AgentConversation;
using Lumo.Api.Services.CodeInterpreter;
using Lumo.Api.Services.ImageGeneration;
using Lumo.Api.Services.Voice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [EnableRateLimiting("ai")]
    [Route("api/chat")]
    public class ChatController : AuthorizedController
    {
        private readonly IAgentConversationService _conversationService;

        public ChatController(IAgentConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        private const int MaxPromptLength = 500;

        /// <summary>
        /// Wysyła wiadomość do asystenta AI i zwraca kompletną odpowiedź.
        /// </summary>
        /// <param name="request">Zapytanie użytkownika.</param>
        /// <param name="cancellationToken">Token anulowania żądania.</param>
        /// <returns>Odpowiedź wygenerowana przez asystenta AI.</returns>
        [HttpPost("generate", Name = "GenerateChatResponse")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Generate([FromBody] ChatMessageRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Prompt jest wymagany.");

            if (request.Text.Length > MaxPromptLength)
                return BadRequest($"Prompt może mieć maksymalnie {MaxPromptLength:N0} znaków.");

            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = "1";

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var response = await _conversationService.SendMessageAsync(userId, request, cancellationToken);

            return Ok(response.Text);
        }

        /// <summary>
        /// Zatwierdza lub odrzuca wykonanie narzędzia przez agenta AI.
        /// </summary>
        [HttpPost("approval", Name = "ApproveChatTool")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Approve([FromBody] ToolApprovalRequest request, CancellationToken cancellationToken = default)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = "1";

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var response = await _conversationService.RespondToApprovalAsync(
                userId,
                request.Approved,
                cancellationToken);

            if (response is null)
                return NotFound("Nie znaleziono oczekującego żądania approval.");

            return Ok(response);
        }
    }

    [ApiController]
    [EnableRateLimiting("ai")]
    [Route("api/image")]
    public class ImageController : AuthorizedController
    {
        private readonly IImageGenerationService _imageGenerationService;

        public ImageController(
            IImageGenerationService imageGenerationService)
        {
            _imageGenerationService = imageGenerationService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
            [FromBody] ImageGenerationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new
                {
                    error = "Prompt jest wymagany."
                });
            }

            try
            {
                var image = await _imageGenerationService.GenerateAsync(
                    request.Prompt,
                    request.Size,
                    cancellationToken);

                return File(
                    image,
                    "image/png",
                    "generated-image.png");
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        error = "Nie udało się wygenerować obrazu.",
                        message = ex.Message
                    });
            }
        }
    }

    public class ImageGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;

        public string? Size { get; set; }
    }

    [ApiController]
    [EnableRateLimiting("ai")]
    [Route("api/voice")]

    public class VoiceController : AuthorizedController
    {
        private readonly IVoiceService _voiceService;
        private readonly IAgentConversationService _agentConversationService;
        private readonly ICodeInterpreterService _codeInterpreterService;

        public VoiceController(
            IVoiceService voiceService, IAgentConversationService agentConversationService, ICodeInterpreterService codeInterpreterService)
        {
            _voiceService = voiceService;
            _agentConversationService = agentConversationService;
            _codeInterpreterService = codeInterpreterService;
        }

        [HttpPost("stt")]

        public async Task<IActionResult> SpeechToText(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Brak pliku audio.");
            }

            await using var stream =
                file.OpenReadStream();

            using var memoryStream =
                new MemoryStream();

            await stream.CopyToAsync(memoryStream);

            var audio =
                memoryStream.ToArray();

            var text =
                await _voiceService.SpeechToTextAsync(
                    audio
                );

            return Ok(new
            {
                fileName = file.FileName,
                size = audio.Length,
                text
            });
        }

        public class CodeInterpreterRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }

        [HttpPost("code")]
        public async Task<IActionResult> ExecuteCode([FromBody] CodeInterpreterRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new
                {
                    error = "Prompt jest wymagany."
                });
            }

            try
            {
                var result =
                    await _codeInterpreterService.ExecuteAsync(
                        request.Prompt,
                        cancellationToken);

                return Ok(new
                {
                    success = true,

                    message = result.Text,

                    containerId = result.ContainerId,

                    fileId = result.FileId,

                    fileName = result.FileName,

                    hasFile =
                        !string.IsNullOrWhiteSpace(result.ContainerId) &&
                        !string.IsNullOrWhiteSpace(result.FileId)
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(
                    StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        error = "Nie udało się wykonać kodu.",
                        message = ex.Message
                    });
            }
        }

        [HttpPost("code2")]
        public async Task<IActionResult> ExecuteCode2(
    [FromBody] CodeInterpreterRequest request,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new
                {
                    error = "Prompt jest wymagany."
                });
            }

            try
            {
                // 1. Wykonujemy prompt przez Code Interpreter
                var result =
                    await _codeInterpreterService.ExecuteAsync(
                        request.Prompt,
                        cancellationToken);

                // 2. Code Interpreter nie wygenerował pliku
                if (string.IsNullOrWhiteSpace(result.ContainerId) ||
                    string.IsNullOrWhiteSpace(result.FileId))
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Text,
                        hasFile = false
                    });
                }

                // 3. Pobieramy plik z kontenera OpenAI
                var file =
                    await _codeInterpreterService.DownloadFileAsync(
                        result.ContainerId,
                        result.FileId,
                        cancellationToken);

                // 4. Nazwa pliku
                var fileName =
                    string.IsNullOrWhiteSpace(result.FileName)
                        ? "generated-file"
                        : Path.GetFileName(result.FileName);

                // 5. Content-Type
                var contentType =
                    GetContentType(fileName);

                // 6. Zwracamy plik BEZPOŚREDNIO do frontendu
                return File(
                    file,
                    contentType,
                    fileName);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(
                    StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        error = "Nie udało się wykonać kodu.",
                        message = ex.Message
                    });
            }
        }

        // ============================================================
        // POBIERANIE PLIKU Z CODE INTERPRETER
        // ============================================================

        [HttpGet("code/file")]
        public async Task<IActionResult> DownloadCodeFile(
     [FromQuery] string containerId,
     [FromQuery] string fileId,
     [FromQuery] string? fileName,
     CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                return BadRequest(new
                {
                    error = "containerId jest wymagany."
                });
            }

            if (string.IsNullOrWhiteSpace(fileId))
            {
                return BadRequest(new
                {
                    error = "fileId jest wymagany."
                });
            }

            try
            {
                var file =
                    await _codeInterpreterService.DownloadFileAsync(
                        containerId,
                        fileId,
                        cancellationToken);

                var name =
                    string.IsNullOrWhiteSpace(fileName)
                        ? "generated-file"
                        : Path.GetFileName(fileName);

                var contentType =
                    GetContentType(name);

                return File(
                    file,
                    contentType,
                    name);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(
                    StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        error = "Nie udało się pobrać pliku.",
                        message = ex.Message
                    });
            }
        }
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName)
                .ToLowerInvariant();

            return extension switch
            {
                ".xlsx" =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                ".xls" =>
                    "application/vnd.ms-excel",

                ".csv" =>
                    "text/csv",

                ".json" =>
                    "application/json",

                ".txt" =>
                    "text/plain",

                ".pdf" =>
                    "application/pdf",

                ".png" =>
                    "image/png",

                ".jpg" or ".jpeg" =>
                    "image/jpeg",

                ".gif" =>
                    "image/gif",

                ".zip" =>
                    "application/zip",

                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".pptx" =>
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation",

                _ =>
                    "application/octet-stream"
            };
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> TestVoice(
    IFormFile file,
    CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new
                {
                    error = "Brak pliku audio."
                });
            }

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var audio = memoryStream.ToArray();

            // 1. STT
            var text = await _voiceService.SpeechToTextAsync(audio);

            if (string.IsNullOrWhiteSpace(text))
            {
                return Ok(new
                {
                    success = false,
                    stage = "STT",
                    error = "Nie udało się rozpoznać mowy."
                });
            }

            var request = new ChatMessageRequest
            {
                Text = text
            };

            // 2. Agent
            var chunks = new List<string>();

            await foreach (var chunk in _agentConversationService.StreamMessageAsync(AuthorizedUser.Id.ToString(), request, cancellationToken))
            {
                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }
            }

            var responseText = string.Concat(chunks);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return Ok(new
                {
                    success = false,
                    stage = "Agent",
                    error = "Agent nie zwrócił odpowiedzi."
                });
            }

            // 3. TTS
            var responseAudio =
                await _voiceService.TextToSpeechAsync(responseText);

            return File(
                responseAudio,
                "audio/wav",
                "response.wav");
        }
    }
}
