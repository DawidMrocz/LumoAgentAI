using Integrations.TikTok.Auth;
using Integrations.TikTok.Services;
using Lumo.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers;

/// <summary>
/// Kontroler TikTok
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TikTokController : ControllerBase
{
    private readonly ITikTokOAuthService _oauthService;
    private readonly ITikTokAccountProvider _accountProvider;
    private readonly ITikTokVideoService _videoService;
    private readonly ITikTokUserService _userService;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<TikTokController> _logger;

    public TikTokController(
        ITikTokOAuthService oauthService,
        ITikTokAccountProvider accountProvider,
        ITikTokVideoService videoService,
        ITikTokUserService userService,
        IUserContextAccessor userContextAccessor,
        ILogger<TikTokController> logger)
    {
        _oauthService = oauthService;
        _accountProvider = accountProvider;
        _videoService = videoService;
        _userService = userService;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Przekierowanie do autoryzacji TikTok
    /// </summary>
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        var url = await _oauthService.CreateAuthorizationUrlAsync(GetUserId(), cancellationToken);
        return Ok(new { url });
    }

    /// <summary>
    /// Callback OAuth TikTok
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        try
        {
            await _oauthService.HandleCallbackAsync(code, state, cancellationToken);

            return Ok(new { message = "TikTok connected successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TikTok callback");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publikuje video na TikTok
    /// </summary>
    [HttpPost("publish")]
    public async Task<ActionResult<PublishVideoResponse>> PublishVideo(
        [FromBody] PublishVideoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _videoService.PublishVideoAsync(request, GetUserId(), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing video to TikTok");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publikuje wiele video
    /// </summary>
    [HttpPost("publish-bulk")]
    public async Task<ActionResult<PublishBulkVideosResponse>> PublishBulkVideos(
        [FromBody] PublishBulkVideosRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _videoService.PublishBulkVideosAsync(request, GetUserId(), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing bulk videos to TikTok");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera listę video użytkownika
    /// </summary>
    [HttpGet("videos")]
    public async Task<ActionResult<List<TikTokVideoInfo>>> GetVideos(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var videos = await _videoService.GetUserVideosAsync(GetUserId(), skip, take, cancellationToken);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TikTok videos");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera szczegóły video
    /// </summary>
    [HttpGet("videos/{videoId}")]
    public async Task<ActionResult<TikTokVideoInfo>> GetVideoDetails(
        string videoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var video = await _videoService.GetVideoDetailsAsync(GetUserId(), videoId, cancellationToken);

            if (video == null)
            {
                return NotFound(new { error = "Video not found" });
            }

            return Ok(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video details");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera statystyki TikTok użytkownika
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<TikTokStatistics>> GetStatistics(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _videoService.GetStatisticsAsync(GetUserId(), from, to, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TikTok statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera informacje o użytkowniku TikTok
    /// </summary>
    [HttpGet("user-info")]
    public async Task<ActionResult<TikTokUserInfo>> GetUserInfo(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userInfo = await _userService.GetUserInfoAsync(GetUserId(), cancellationToken);

            if (userInfo == null)
            {
                return NotFound(new { error = "TikTok user info not found" });
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TikTok user info");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sprawdza czy użytkownik ma połączone konto TikTok
    /// </summary>
    [HttpGet("is-connected")]
    public async Task<ActionResult<bool>> IsConnected(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountProvider.GetAsync(GetUserId(), cancellationToken);
            return Ok(new { connected = account is not null });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking TikTok connection");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rozłącza konto TikTok
    /// </summary>
    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _accountProvider.DeleteAsync(GetUserId(), cancellationToken);

            return Ok(new { message = "TikTok disconnected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting TikTok");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Odświeża statystyki video
    /// </summary>
    [HttpPost("refresh-statistics")]
    public async Task<IActionResult> RefreshStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _videoService.RefreshVideoStatisticsAsync(GetUserId(), cancellationToken);

            return Ok(new { message = "Statistics refreshed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    private int GetUserId()
    {
        var user = _userContextAccessor.GetUser();

        if (!int.TryParse(user.Id, out var userId))
        {
            throw new UnauthorizedAccessException("Nie można określić UserId.");
        }

        return userId;
    }
}
