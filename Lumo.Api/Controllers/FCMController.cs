using Lumo.Api.Data.User;
using Lumo.Api.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Route("api/fcm")]
    [Authorize]
    public class FCMController : AuthorizedController
    {
        private readonly AppDbContext _db;
        private readonly ILogger<FCMController> _logger;

        public FCMController(AppDbContext db, ILogger<FCMController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterFcmTokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new
                {
                    message = "FCM token jest wymagany."
                });
            }


            if (string.IsNullOrWhiteSpace(AuthorizedUser.Id))
            {
                return Unauthorized(new
                {
                    message = "Nie można ustalić użytkownika."
                });
            }

            var existingToken = await _db.DeviceTokens.FirstOrDefaultAsync(x => x.Token == request.Token);

            if (existingToken != null)
            {
                // Token już istnieje.
                // Jeżeli należy do tego samego użytkownika,
                // nie musimy nic robić.
                if (existingToken.UserOid == AuthorizedUser.Id)
                {
                    return Ok(new
                    {
                        message = "Token już jest zarejestrowany."
                    });
                }

                // Ten sam token został przypisany do innego użytkownika.
                existingToken.UserOid = AuthorizedUser.Id;

                await _db.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    message = "Token został przypisany do użytkownika."
                });
            }

            var deviceToken = new DeviceToken
            {
                UserOid = AuthorizedUser.Id,
                Token = request.Token
            };

            _db.DeviceTokens.Add(deviceToken);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Zarejestrowano FCM token dla użytkownika {UserOid}",
                AuthorizedUser.Id);

            return Ok(new
            {
                message = "Token FCM został zarejestrowany."
            });
        }
    }

    public sealed class RegisterFcmTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}