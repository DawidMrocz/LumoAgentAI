using Integrations.Przelewy24.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integrations.Przelewy24;

/// <summary>
/// Implementacja serwisu Przelewy24
/// </summary>
internal sealed class Przelewy24Service : IPrzelewy24Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<Przelewy24Service> _logger;

    private readonly int _merchantId;
    private readonly int _posId;
    private readonly string _crcKey;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _currency;
    private readonly string _country;
    private readonly string _language;
    private readonly string _returnUrl;
    private readonly string _statusUrl;

    public Przelewy24Service(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IPaymentRepository paymentRepository,
        ILogger<Przelewy24Service> logger)
    {
        _httpClientFactory = httpClientFactory;
        _paymentRepository = paymentRepository;
        _logger = logger;

        _merchantId = configuration.GetValue<int>("Przelewy24:MerchantId");
        _posId = configuration.GetValue<int>("Przelewy24:PosId");
        _crcKey = configuration["Przelewy24:CrcKey"] ?? throw new InvalidOperationException("Przelewy24:CrcKey not configured");
        _apiKey = configuration["Przelewy24:ApiKey"] ?? throw new InvalidOperationException("Przelewy24:ApiKey not configured");

        var isSandbox = configuration.GetValue<bool>("Przelewy24:IsSandbox", true);
        _baseUrl = isSandbox
            ? configuration["Przelewy24:SandboxUrl"] ?? "https://sandbox.przelewy24.pl"
            : configuration["Przelewy24:ProductionUrl"] ?? "https://secure.przelewy24.pl";

        _currency = configuration["Przelewy24:Currency"] ?? "PLN";
        _country = configuration["Przelewy24:Country"] ?? "PL";
        _language = configuration["Przelewy24:Language"] ?? "pl";
        _returnUrl = configuration["Przelewy24:ReturnUrl"] ?? throw new InvalidOperationException("Przelewy24:ReturnUrl not configured");
        _statusUrl = configuration["Przelewy24:StatusUrl"] ?? throw new InvalidOperationException("Przelewy24:StatusUrl not configured");
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(
        string userId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var sessionId = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}";

        var payment = new Payment
        {
            UserId = userId,
            SessionId = sessionId,
            Amount = request.Amount,
            Currency = _currency,
            Description = request.Description ?? "Płatność",
            Email = request.Email,
            ClientName = request.ClientName,
            Phone = request.Phone,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        _logger.LogInformation("Created payment {PaymentId} for user {UserId}, sessionId: {SessionId}",
            payment.Id, userId, sessionId);

        var sign = CalculateRegisterSign(sessionId, request.Amount);

        var registerRequest = new P24RegisterRequest
        {
            MerchantId = _merchantId,
            PosId = _posId,
            SessionId = sessionId,
            Amount = request.Amount,
            Currency = _currency,
            Description = payment.Description ?? string.Empty,
            Email = request.Email,
            Client = request.ClientName,
            Phone = request.Phone,
            Country = _country,
            Language = _language,
            UrlReturn = _returnUrl,
            UrlStatus = _statusUrl,
            Method = request.PreferredPaymentMethod,
            Sign = sign
        };

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_posId}:{_apiKey}"))}");

        var response = await httpClient.PostAsJsonAsync(
            $"{_baseUrl}/api/v1/transaction/register",
            registerRequest,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Przelewy24 registration failed: {StatusCode} - {Content}",
                response.StatusCode, content);
            throw new InvalidOperationException($"Przelewy24 registration failed: {content}");
        }

        var registerResponse = JsonSerializer.Deserialize<P24RegisterResponse>(content);

        if (registerResponse?.Data?.Token == null)
        {
            _logger.LogError("Przelewy24 returned no token: {Content}", content);
            throw new InvalidOperationException("Przelewy24 returned no token");
        }

        payment.Token = registerResponse.Data.Token;
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogInformation("Przelewy24 token obtained for payment {PaymentId}: {Token}",
            payment.Id, registerResponse.Data.Token);

        return new CreatePaymentResponse
        {
            Token = registerResponse.Data.Token,
            RedirectUrl = $"{_baseUrl}/trnRequest/{registerResponse.Data.Token}",
            PaymentId = payment.Id,
            SessionId = sessionId
        };
    }

    public async Task<bool> VerifyPaymentAsync(
        string sessionId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for sessionId: {SessionId}", sessionId);
            return false;
        }

        var sign = CalculateVerifySign(sessionId, orderId, payment.Amount);

        var verifyRequest = new P24VerifyRequest
        {
            MerchantId = _merchantId,
            PosId = _posId,
            SessionId = sessionId,
            Amount = payment.Amount,
            Currency = _currency,
            OrderId = orderId,
            Sign = sign
        };

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_posId}:{_apiKey}"))}");

        var response = await httpClient.PutAsJsonAsync(
            $"{_baseUrl}/api/v1/transaction/verify",
            verifyRequest,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Przelewy24 verification failed for payment {PaymentId}: {StatusCode} - {Content}",
                payment.Id, response.StatusCode, content);
            return false;
        }

        var verifyResponse = JsonSerializer.Deserialize<P24VerifyResponse>(content);

        if (verifyResponse?.Data?.Status == "success")
        {
            payment.Status = PaymentStatus.Completed;
            payment.P24OrderId = orderId;
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} verified successfully", payment.Id);
            return true;
        }

        _logger.LogWarning("Payment {PaymentId} verification returned status: {Status}",
            payment.Id, verifyResponse?.Data?.Status);
        return false;
    }

    public async Task<bool> HandleNotificationAsync(
        P24NotificationRequest notification,
        CancellationToken cancellationToken = default)
    {
        var expectedSign = CalculateNotificationSign(notification);
        if (expectedSign != notification.Sign)
        {
            _logger.LogWarning("Invalid notification sign for sessionId: {SessionId}", notification.SessionId);
            return false;
        }

        var payment = await _paymentRepository.GetBySessionIdAsync(notification.SessionId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for notification sessionId: {SessionId}", notification.SessionId);
            return false;
        }

        payment.P24OrderId = notification.OrderId;
        payment.Statement = notification.Statement;
        payment.Method = (PaymentMethod)notification.MethodId;
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        var verified = await VerifyPaymentAsync(notification.SessionId, notification.OrderId, cancellationToken);

        if (!verified)
        {
            _logger.LogWarning("Notification received but verification failed for payment {PaymentId}", payment.Id);
        }

        return verified;
    }

    public Task<Payment?> GetPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        return _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
    }

    public Task<Payment?> GetPaymentBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return _paymentRepository.GetBySessionIdAsync(sessionId, cancellationToken);
    }

    // ===== SIGN CALCULATION =====

    private string CalculateRegisterSign(string sessionId, int amount)
    {
        var data = $"{{\"sessionId\":\"{sessionId}\",\"merchantId\":{_merchantId},\"amount\":{amount},\"currency\":\"{_currency}\",\"crc\":\"{_crcKey}\"}}";
        return CalculateSHA384(data);
    }

    private string CalculateVerifySign(string sessionId, int orderId, int amount)
    {
        var data = $"{{\"sessionId\":\"{sessionId}\",\"orderId\":{orderId},\"amount\":{amount},\"currency\":\"{_currency}\",\"crc\":\"{_crcKey}\"}}";
        return CalculateSHA384(data);
    }

    private string CalculateNotificationSign(P24NotificationRequest notification)
    {
        var data = $"{{\"sessionId\":\"{notification.SessionId}\",\"orderId\":{notification.OrderId},\"amount\":{notification.Amount},\"currency\":\"{notification.Currency}\",\"crc\":\"{_crcKey}\"}}";
        return CalculateSHA384(data);
    }

    private static string CalculateSHA384(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA384.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
