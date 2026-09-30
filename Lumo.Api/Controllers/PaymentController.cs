using Integrations.Przelewy24;
using Integrations.Przelewy24.Models;
using Lumo.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers;

/// <summary>
/// Kontroler obsługi płatności Przelewy24
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPrzelewy24Service _przelewy24Service;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPrzelewy24Service przelewy24Service,
        IUserContextAccessor userContextAccessor,
        ILogger<PaymentController> logger)
    {
        _przelewy24Service = przelewy24Service;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Tworzy nową płatność
    /// </summary>
    /// <param name="request">Dane płatności</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Dane do przekierowania użytkownika</returns>
    [HttpPost("create")]
    [Authorize]
    public async Task<ActionResult<CreatePaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _userContextAccessor.GetUser().Id;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated");
        }

        try
        {
            var response = await _przelewy24Service.CreatePaymentAsync(userId, request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to create payment", message = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint return - użytkownik wraca po opłaceniu płatności
    /// </summary>
    [HttpGet("return")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentReturn(
        [FromQuery] string? sessionId,
        [FromQuery] int? orderId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || !orderId.HasValue)
        {
            return BadRequest("Missing sessionId or orderId");
        }

        try
        {
            var verified = await _przelewy24Service.VerifyPaymentAsync(sessionId, orderId.Value, cancellationToken);

            if (verified)
            {
                // Możesz przekierować użytkownika na stronę sukcesu
                return Redirect($"{Request.Scheme}://{Request.Host}/payment/success?sessionId={sessionId}");
            }

            return Redirect($"{Request.Scheme}://{Request.Host}/payment/failed?sessionId={sessionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment return for sessionId: {SessionId}", sessionId);
            return Redirect($"{Request.Scheme}://{Request.Host}/payment/error");
        }
    }

    /// <summary>
    /// Webhook status - powiadomienie zwrotne z Przelewy24
    /// </summary>
    [HttpPost("status")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentStatus(
        [FromForm] P24NotificationRequest notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _przelewy24Service.HandleNotificationAsync(notification, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Notification processed successfully for sessionId: {SessionId}",
                    notification.SessionId);
                return Ok();
            }

            _logger.LogWarning("Notification processing failed for sessionId: {SessionId}",
                notification.SessionId);
            return BadRequest("Notification verification failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification for sessionId: {SessionId}",
                notification.SessionId);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Pobiera status płatności
    /// </summary>
    [HttpGet("{paymentId}")]
    [Authorize]
    public async Task<IActionResult> GetPayment(int paymentId, CancellationToken cancellationToken)
    {
        var userId = _userContextAccessor.GetUser().Id;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var payment = await _przelewy24Service.GetPaymentAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return NotFound();
        }

        // Sprawdź czy płatność należy do użytkownika
        if (payment.UserId != userId)
        {
            return Forbid();
        }

        return Ok(new
        {
            payment.Id,
            payment.SessionId,
            payment.Amount,
            payment.Currency,
            payment.Description,
            payment.Status,
            payment.Method,
            payment.CreatedAt,
            payment.PaidAt
        });
    }

    /// <summary>
    /// Pobiera listę płatności użytkownika
    /// </summary>
    [HttpGet("my-payments")]
    [Authorize]
    public async Task<IActionResult> GetMyPayments(CancellationToken cancellationToken)
    {
        var userId = _userContextAccessor.GetUser().Id;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        // TODO: Add GetUserPayments method to service
        return Ok(new { message = "Not implemented yet" });
    }
}
