using System.Text.Json.Serialization;

namespace Integrations.Przelewy24.Models;

public sealed class CreatePaymentRequest
{
    public int Amount { get; set; }

    public string? Description { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? Phone { get; set; }

    public int? PreferredPaymentMethod { get; set; }
}

public sealed class CreatePaymentResponse
{
    public string Token { get; set; } = string.Empty;

    public string RedirectUrl { get; set; } = string.Empty;

    public int PaymentId { get; set; }

    public string SessionId { get; set; } = string.Empty;
}

public sealed class P24NotificationRequest
{
    [JsonPropertyName("merchantId")]
    public int MerchantId { get; set; }

    [JsonPropertyName("posId")]
    public int PosId { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("methodId")]
    public int MethodId { get; set; }

    [JsonPropertyName("statement")]
    public string? Statement { get; set; }

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;
}

internal sealed class P24RegisterRequest
{
    [JsonPropertyName("merchantId")]
    public int MerchantId { get; set; }

    [JsonPropertyName("posId")]
    public int PosId { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("client")]
    public string? Client { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("urlReturn")]
    public string UrlReturn { get; set; } = string.Empty;

    [JsonPropertyName("urlStatus")]
    public string UrlStatus { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public int? Method { get; set; }

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;
}

internal sealed class P24RegisterResponse
{
    [JsonPropertyName("data")]
    public P24RegisterData? Data { get; set; }
}

internal sealed class P24RegisterData
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

internal sealed class P24VerifyRequest
{
    [JsonPropertyName("merchantId")]
    public int MerchantId { get; set; }

    [JsonPropertyName("posId")]
    public int PosId { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;
}

internal sealed class P24VerifyResponse
{
    [JsonPropertyName("data")]
    public P24VerifyData? Data { get; set; }
}

internal sealed class P24VerifyData
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
