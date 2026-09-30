using Integrations.Meta.Auth;
using Integrations.Meta.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetaController : ControllerBase
{
    private readonly IMetaOAuthService _oauthService;
    private readonly IMetaPageService _pageService;
    private readonly IMetaInstagramService _instagramService;

    public MetaController(
        IMetaOAuthService oauthService,
        IMetaPageService pageService,
        IMetaInstagramService instagramService)
    {
        _oauthService = oauthService;
        _pageService = pageService;
        _instagramService = instagramService;
    }

    [HttpGet("oauth/url")]
    public async Task<IActionResult> GetLoginUrl(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var url =
            await _oauthService.CreateAuthorizationUrlAsync(
                userId,
                cancellationToken);

        return Ok(new { url });
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Missing code");

        if (string.IsNullOrWhiteSpace(state))
            return BadRequest("Missing state");

        await _oauthService.HandleCallbackAsync(
            code,
            state,
            cancellationToken);

        return Ok(new
        {
            success = true
        });
    }

    [HttpGet("pages")]
    public async Task<IActionResult> Pages(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var pages =
            await _pageService.GetUserPagesAsync(
                userId,
                cancellationToken);

        return Ok(pages);
    }

    [HttpGet("ig/account")]
    public async Task<IActionResult> InstagramAccount(
        [FromQuery] string pageId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pageId))
            return BadRequest("Missing pageId");

        var userId = GetUserId();

        var account =
            await _pageService
                .GetInstagramBusinessAccountAsync(
                    userId,
                    pageId,
                    cancellationToken);

        if (account is null)
            return NotFound(
                "No instagram_business_account found for page");

        return Ok(account);
    }

    [HttpGet("ig/media")]
    public async Task<IActionResult> GetInstagramMedia(
        [FromQuery] string igUserId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(igUserId))
            return BadRequest("Missing igUserId");

        var userId = GetUserId();

        var media =
            await _instagramService.GetMediaAsync(
                userId,
                igUserId,
                limit: limit,
                cancellationToken: cancellationToken);

        return Ok(media);
    }

    public sealed class PublishRequest
    {
        public string IgUserId { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }
    }

    [HttpPost("ig/publish")]
    public async Task<IActionResult> PublishInstagramImage(
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IgUserId))
            return BadRequest("Missing igUserId");

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
            return BadRequest("Missing imageUrl");

        var userId = GetUserId();

        var postId =
            await _instagramService.PublishImageAsync(
                userId,
                request.IgUserId,
                request.ImageUrl,
                request.Caption ?? string.Empty,
                cancellationToken);

        return Ok(new
        {
            id = postId
        });
    }

    private int GetUserId()
    {
        var claim =
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier);

        if (claim is null ||
            !int.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Nie można określić UserId.");
        }

        return userId;
    }
}