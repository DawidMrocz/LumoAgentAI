using Lumo.Api.ApiModels.User;
using Lumo.Api.DTO.User;
using Lumo.Api.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lumo.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : AuthorizedController
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccessController> _logger;

        public UserController(IUserService userService, ILogger<AccessController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Odświeżanie tokenów JWT
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] ApiModels.User.RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _userService.RefreshToken(request.RefreshToken, ip);

            if (result == null)
                return Unauthorized("Invalid or expired refresh token");

            return Ok(result);
        }

        /// <summary>
        /// Logowanie użytkownika do systemu
        /// </summary>
        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] ApiModels.User.LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var authResult = await _userService.Login(request, ip);
            if (authResult == null)
                return Unauthorized("Invalid login or password");

            return Ok(authResult);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _userService.RevokeToken(request.RefreshToken, CurrentUserId);
            return Ok(new { message = "Wylogowano pomyślnie" });
        }

        [HttpPost("logout/all")]
        public async Task<IActionResult> LogoutAll()
        {
            await _userService.RevokeAllTokens(CurrentUserId);
            return Ok(new { message = "Wylogowano ze wszystkich urządzeń" });
        }

        /// <summary>
        /// Rejestracja użytkownika
        /// </summary>
        [EnableRateLimiting("auth")]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ApiModels.User.RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.Register(request);
            return Ok("User registered successfully");
        }

        /// <summary>
        /// Aktywacja konta użytkownika na podstawie tokenu z maila
        /// </summary>
        [HttpGet("activate")]
        [AllowAnonymous]
        public async Task<IActionResult> Activate([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Activation token is required");

            await _userService.ActivateAccount(token);

            return Ok("Account activated successfully");
        }

        [EnableRateLimiting("auth")]
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var authResult = await _userService.GoogleLogin(request.IdToken, ip);
            if (authResult == null)
                return Unauthorized("Invalid login or password");

            return Ok(authResult);
        }

        [EnableRateLimiting("auth")]
        [HttpPost("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var authResult = await _userService.FacebookLogin(request.AccessToken, ip);
            if (authResult == null)
                return Unauthorized("Invalid login or password");

            return Ok(authResult);
        }

        /// <summary>
        /// Wysyła email do resetu hasła
        /// </summary>
        [EnableRateLimiting("auth")]
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ApiModels.User.ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.ForgotPassword(request.Email);
            return Ok(new { message = "If the email exists, reset instructions were sent." });
        }

        /// <summary>
        /// Reset hasła przy użyciu tokenu z maila
        /// </summary>
        [EnableRateLimiting("auth")]
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ApiModels.User.ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.ResetPassword(request.Token, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }

        /// <summary>
        /// Ponowne wysłanie maila aktywacyjnego
        /// </summary>
        [EnableRateLimiting("auth")]
        [HttpPost("resend-activation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendActivation([FromBody] ResendActivationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.ResendActivation(request.Email);

            return Ok(new { message = "Activation email sent if account exists." });
        }

        /// <summary>
        /// Zwraca profil zalogowanego użytkownika
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ProfileResponse>> Me()
        {
            var profile = await _userService.Profile(CurrentUserId);

            return Ok(profile);
        }

        /// <summary>
        /// Aktualizacja danych użytkownika
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.UpdateProfile(CurrentUserId, request);

            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>
        /// Zmiana hasła dla zalogowanego użytkownika
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.ChangePassword(CurrentUserId, request.CurrentPassword, request.NewPassword);

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Ustawienie hasła dla konta założonego przez Google/Facebook (które hasła nie ma).
        /// </summary>
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.SetPassword(CurrentUserId, request.NewPassword);

            return Ok(new { message = "Password set successfully" });
        }

        /// <summary>
        /// Odłączenie logowania społecznościowego ("google" albo "facebook").
        /// </summary>
        [HttpDelete("external-login/{provider}")]
        public async Task<IActionResult> UnlinkExternalLogin([FromRoute] string provider)
        {
            await _userService.UnlinkExternalLogin(CurrentUserId, provider);

            return Ok(new { message = $"{provider} unlinked successfully" });
        }

        /// <summary>
        /// Lista aktywnych sesji (urządzeń) zalogowanego użytkownika.
        /// </summary>
        [HttpGet("sessions")]
        public async Task<ActionResult<List<SessionResponse>>> GetSessions([FromQuery] string? currentRefreshToken = null)
        {
            var sessions = await _userService.GetSessions(CurrentUserId, currentRefreshToken);

            return Ok(sessions);
        }

        /// <summary>
        /// Trwałe usunięcie konta zalogowanego użytkownika.
        /// </summary>
        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            await _userService.DeleteAccount(CurrentUserId, request?.CurrentPassword);

            return Ok(new { message = "Account deleted successfully" });
        }
    }
}
