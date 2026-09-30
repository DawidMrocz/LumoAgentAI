using Integrations.Google.API.Ads;
using Integrations.Google.API.Ads.Models;
using Integrations.Google.API.BusinessProfile;
using Integrations.Google.API.BusinessProfile.Models;
using Integrations.Google.API.Drive;
using Integrations.Google.API.Drive.Models;
using Integrations.Google.API.Drive.Models.Requests;
using Integrations.Google.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : AuthorizedController
    {
        private readonly IGoogleOAuthService _googleOAuthService;

        public GoogleAuthController(IGoogleOAuthService googleOAuthService)
        {
            _googleOAuthService = googleOAuthService;
        }

        /// <summary>
        /// Zwraca URL ekranu zgody Google dla ZALOGOWANEGO użytkownika.
        /// Celowo zwraca JSON, a nie przekierowanie: nawigacja przeglądarki nie niesie
        /// nagłówka Authorization, więc front musi pobrać URL fetchem z tokenem,
        /// a dopiero potem wykonać window.location.href = url.
        /// </summary>
        [HttpGet("connect")]
        public async Task<IActionResult> Connect(CancellationToken cancellationToken)
        {
            var url = await _googleOAuthService.CreateAuthorizationUrlAsync(CurrentUserId, cancellationToken);

            return Ok(new { url });
        }

        /// <summary>
        /// Callback OAuth - musi pozostać anonimowy, bo to Google przekierowuje tu przeglądarkę.
        /// Tożsamość użytkownika pochodzi z kryptograficznie chronionego parametru "state",
        /// wystawionego w Connect dla konkretnego zalogowanego użytkownika.
        /// </summary>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
        {
            await _googleOAuthService.HandleCallbackAsync(code, state, cancellationToken);
            return Ok(new { connected = true });
        }
    }

    [ApiController]
    [Route("api/drive")]
    public class GoogleDriveController : AuthorizedController
    {
        private readonly IGoogleDriveService _googleDriveService;

        public GoogleDriveController(IGoogleDriveService googleDriveService)
        {
            _googleDriveService = googleDriveService;
        }

        [HttpGet("files")]
        public async Task<ActionResult<IReadOnlyList<DriveFile>>> ListFiles(
            [FromQuery] string? parentFolderId = null,
            [FromQuery] int maxResults = 100,
            CancellationToken cancellationToken = default)
        {
            var files = await _googleDriveService.ListFilesAsync(
                CurrentUserId,
                parentFolderId,
                maxResults,
                cancellationToken);

            return Ok(files);
        }

        [HttpGet("files/{fileId}")]
        public async Task<ActionResult<DriveFile>> GetFile(
            [FromRoute] string fileId,
            CancellationToken cancellationToken = default)
        {
            var file = await _googleDriveService.GetFileAsync(
                CurrentUserId,
                fileId,
                cancellationToken);

            if (file is null)
                return NotFound();

            return Ok(file);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<DriveFile>>> SearchFiles(
            [FromQuery] string query,
            [FromQuery] int maxResults = 50,
            CancellationToken cancellationToken = default)
        {
            var files = await _googleDriveService.SearchFilesAsync(
                CurrentUserId,
                query,
                maxResults,
                cancellationToken);

            return Ok(files);
        }

        [HttpPost("folders")]
        public async Task<ActionResult<DriveFile>> CreateFolder(
            [FromBody] CreateFolderRequest request,
            CancellationToken cancellationToken = default)
        {
            var folder = await _googleDriveService.CreateFolderAsync(
                CurrentUserId,
                request,
                cancellationToken);

            return Ok(folder);
        }

        [HttpDelete("files/{fileId}")]
        public async Task<IActionResult> DeleteFile(
            [FromRoute] string fileId,
            CancellationToken cancellationToken = default)
        {
            await _googleDriveService.DeleteFileAsync(
                CurrentUserId,
                fileId,
                cancellationToken);

            return NoContent();
        }
    }

    [ApiController]
    [Route("api/ads")]
    public class GoogleAdsController : AuthorizedController
    {
        private readonly IGoogleAdsService _googleAdsService;

        public GoogleAdsController(IGoogleAdsService googleAdsService)
        {
            _googleAdsService = googleAdsService;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<IReadOnlyList<AdsAccount>>> ListAccounts(
            CancellationToken cancellationToken = default)
        {
            var accounts = await _googleAdsService.ListAccessibleAccountsAsync(
                CurrentUserId,
                cancellationToken);

            return Ok(accounts);
        }

        [HttpGet("accounts/{customerId}/campaigns")]
        public async Task<ActionResult<IReadOnlyList<AdsCampaign>>> GetCampaigns(
            [FromRoute] string customerId,
            [FromQuery] DateOnly? dateFrom = null,
            [FromQuery] DateOnly? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var campaigns = await _googleAdsService.GetCampaignsAsync(
                CurrentUserId,
                customerId,
                dateFrom,
                dateTo,
                cancellationToken);

            return Ok(campaigns);
        }
    }

    [ApiController]
    [Route("api/business-profile")]
    public class GoogleBusinessProfileController : AuthorizedController
    {
        private readonly IGoogleBusinessProfileService _businessProfileService;

        public GoogleBusinessProfileController(IGoogleBusinessProfileService businessProfileService)
        {
            _businessProfileService = businessProfileService;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<IReadOnlyList<BusinessAccount>>> ListAccounts(
            CancellationToken cancellationToken = default)
        {
            var accounts = await _businessProfileService.ListAccountsAsync(
                CurrentUserId,
                cancellationToken);

            return Ok(accounts);
        }

        [HttpGet("accounts/{accountId}/locations")]
        public async Task<ActionResult<IReadOnlyList<BusinessLocation>>> ListLocations(
            [FromRoute] string accountId,
            CancellationToken cancellationToken = default)
        {
            var locations = await _businessProfileService.ListLocationsAsync(
                CurrentUserId,
                $"accounts/{accountId}",
                cancellationToken);

            return Ok(locations);
        }

        [HttpGet("locations/{locationId}/performance")]
        public async Task<ActionResult<IReadOnlyList<LocationMetricPoint>>> GetLocationPerformance(
            [FromRoute] string locationId,
            [FromQuery] DateOnly? dateFrom = null,
            [FromQuery] DateOnly? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var metrics = await _businessProfileService.GetLocationPerformanceAsync(
                CurrentUserId,
                $"locations/{locationId}",
                dateFrom,
                dateTo,
                cancellationToken);

            return Ok(metrics);
        }
    }
}
