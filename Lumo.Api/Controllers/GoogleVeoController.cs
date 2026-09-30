using Integrations.Google.API.Veo;
using Integrations.Google.API.Veo.Models;
using Lumo.Api.Helpers;
using Lumo.Api.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers;

/// <summary>
/// Kontroler Google Veo - generowanie video AI
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoogleVeoController : ControllerBase
{
    private readonly IGoogleVeoService _veoService;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<GoogleVeoController> _logger;

    public GoogleVeoController(
        IGoogleVeoService veoService,
        IUserContextAccessor userContextAccessor,
        ILogger<GoogleVeoController> logger)
    {
        _veoService = veoService;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(_userContextAccessor.GetUser().Id, out userId);
    }

    /// <summary>
    /// Generuje video z tekstu
    /// </summary>
    /// <param name="request">Request generowania</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateVideo(
        [FromBody] GenerateVideoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.GenerateVideoAsync(request, userId, cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Generuje video używając presetu
    /// </summary>
    /// <param name="presetName">Nazwa presetu (advertisement, socialmedia, cinematic)</param>
    /// <param name="prompt">Prompt tekstowy</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("generate/preset/{presetName}")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateFromPreset(
        [FromRoute] string presetName,
        [FromBody] string prompt,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.GenerateVideoFromPresetAsync(
                presetName,
                prompt,
                userId,
                cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video from preset");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Generuje video z obrazu (Image → Video)
    /// </summary>
    /// <param name="request">Request z obrazem źródłowym i promptem animacji</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("generate/from-image")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateFromImage(
        [FromBody] ImageToVideoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.GenerateVideoFromImageAsync(
                request,
                userId,
                cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video from image");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Generuje video z wielu obrazów (Multi-Image → Video)
    /// Przykład: 2 zdjęcia osób które się przytulają
    /// </summary>
    /// <param name="request">Request z wieloma obrazami i promptem sceny</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("generate/from-multiple-images")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateFromMultipleImages(
        [FromBody] MultiImageToVideoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.GenerateVideoFromMultipleImagesAsync(
                request,
                userId,
                cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video from multiple images");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Sprawdza status generowania
    /// </summary>
    /// <param name="jobId">ID zadania</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Status generowania</returns>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(
        [FromRoute] string jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _veoService.GetGenerationStatusAsync(jobId, cancellationToken);

            if (!response.Success && response.Error == "Video nie znalezione")
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobiera listę wygenerowanych video użytkownika
    /// </summary>
    /// <param name="skip">Liczba rekordów do pominięcia</param>
    /// <param name="take">Liczba rekordów do pobrania</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Lista video</returns>
    [HttpGet("videos")]
    [ProducesResponseType(typeof(List<VeoVideoInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVideos(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var videos = await _veoService.GetUserVideosAsync(userId, skip, take, cancellationToken);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user videos");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobiera szczegóły konkretnego video
    /// </summary>
    /// <param name="videoId">ID video</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Szczegóły video</returns>
    [HttpGet("videos/{videoId}")]
    [ProducesResponseType(typeof(VeoVideoInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideoDetails(
        [FromRoute] int videoId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var video = await _veoService.GetVideoDetailsAsync(userId, videoId, cancellationToken);

            if (video == null)
                return NotFound(new { error = "Video not found" });

            return Ok(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video details");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobiera statystyki użytkownika
    /// </summary>
    /// <param name="from">Data początkowa</param>
    /// <param name="to">Data końcowa</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Statystyki</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(VeoStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var stats = await _veoService.GetStatisticsAsync(userId, from, to, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Pobiera dostępne presety
    /// </summary>
    /// <returns>Lista presetów</returns>
    [HttpGet("presets")]
    [ProducesResponseType(typeof(List<VeoPreset>), StatusCodes.Status200OK)]
    public IActionResult GetPresets()
    {
        try
        {
            var presets = _veoService.GetPresets();
            return Ok(presets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting presets");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Anuluje generowanie video
    /// </summary>
    /// <param name="jobId">ID zadania</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Sukces/porażka</returns>
    [HttpPost("cancel/{jobId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelGeneration(
        [FromRoute] string jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _veoService.CancelGenerationAsync(jobId, cancellationToken);

            if (!success)
                return NotFound(new { error = "Job not found or already completed" });

            return Ok(new { message = "Generation cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling generation");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Rozszerza istniejące video
    /// </summary>
    /// <param name="request">Request rozszerzenia</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("extend")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExtendVideo(
        [FromBody] ExtendVideoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.ExtendVideoAsync(request, userId, cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending video");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Przekształca styl video
    /// </summary>
    /// <param name="request">Request transformacji</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Response z job ID</returns>
    [HttpPost("style-transfer")]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferStyle(
        [FromBody] StyleTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _veoService.TransferStyleAsync(request, userId, cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring style");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
