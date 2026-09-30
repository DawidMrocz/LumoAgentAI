using Integrations.Twilio.Services;
using Lumo.Api.Helpers;
using Lumo.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers;

/// <summary>
/// Kontroler SMS
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<SmsController> _logger;

    public SmsController(
        ISmsService smsService,
        IUserContextAccessor userContextAccessor,
        ILogger<SmsController> logger)
    {
        _smsService = smsService;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Wysyła SMS
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult<SendSmsResponse>> SendSms(
        [FromBody] SendSmsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _smsService.SendSmsAsync(request, GetUserId(), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania SMS");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Wysyła SMS-y masowo
    /// </summary>
    [HttpPost("send-bulk")]
    public async Task<ActionResult<SendBulkSmsResponse>> SendBulkSms(
        [FromBody] SendBulkSmsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _smsService.SendBulkSmsAsync(request, GetUserId(), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania SMS-ów masowych");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Wysyła SMS z szablonu
    /// </summary>
    [HttpPost("send-template")]
    public async Task<ActionResult<SendSmsResponse>> SendFromTemplate(
        [FromQuery] string templateName,
        [FromQuery] string to,
        [FromBody] Dictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _smsService.SendFromTemplateAsync(
                templateName,
                to,
                variables,
                GetUserId(),
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania SMS z szablonu");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sprawdza status wiadomości
    /// </summary>
    [HttpGet("status/{messageId}")]
    public async Task<IActionResult> GetMessageStatus(
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _smsService.GetMessageStatusAsync(messageId, cancellationToken);
            return Ok(new { messageId, status = status.ToString() });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania statusu SMS");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera historię SMS-ów użytkownika
    /// </summary>
    [HttpGet("my-messages")]
    public async Task<ActionResult<List<SmsMessageInfo>>> GetMyMessages(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = await _smsService.GetUserMessagesAsync(GetUserId(), skip, take, cancellationToken);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania historii SMS");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera statystyki użycia SMS
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<SmsStatistics>> GetStatistics(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _smsService.GetStatisticsAsync(GetUserId(), from, to, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania statystyk SMS");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pobiera dostępne szablony SMS
    /// </summary>
    [HttpGet("templates")]
    public ActionResult<List<SmsTemplate>> GetTemplates()
    {
        var templates = new List<SmsTemplate>
        {
            new() { Name = "welcome", Content = "Witaj {name}! Dziękujemy za rejestrację w Lumo.", Description = "Wiadomość powitalna", IsActive = true },
            new() { Name = "verification", Content = "Twój kod weryfikacyjny: {code}. Ważny przez {minutes} minut.", Description = "Kod weryfikacyjny", IsActive = true },
            new() { Name = "password_reset", Content = "Kod resetowania hasła: {code}. Jeśli to nie Ty, zignoruj tę wiadomość.", Description = "Reset hasła", IsActive = true },
            new() { Name = "notification", Content = "{message}", Description = "Ogólne powiadomienie", IsActive = true }
        };

        return Ok(templates);
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
