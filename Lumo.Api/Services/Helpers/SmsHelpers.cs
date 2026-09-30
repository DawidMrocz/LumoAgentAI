using Integrations.Twilio.Services;

namespace Lumo.Api.Services.Helpers;

/// <summary>
/// Pomocnicze metody dla typowych scenariuszy wysyłki SMS
/// </summary>
public static class SmsHelpers
{
    /// <summary>
    /// Wysyła kod weryfikacyjny dla nowego użytkownika
    /// </summary>
    public static async Task<string> SendVerificationCodeAsync(
        this ISmsService smsService,
        string phoneNumber,
        string code,
        int validMinutes = 10,
        int? userId = null)
    {
        var response = await smsService.SendFromTemplateAsync(
            "verification",
            phoneNumber,
            new Dictionary<string, string>
            {
                ["code"] = code,
                ["minutes"] = validMinutes.ToString()
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła wiadomość powitalną dla nowego użytkownika
    /// </summary>
    public static async Task<string> SendWelcomeMessageAsync(
        this ISmsService smsService,
        string phoneNumber,
        string userName,
        int? userId = null)
    {
        var response = await smsService.SendFromTemplateAsync(
            "welcome",
            phoneNumber,
            new Dictionary<string, string>
            {
                ["name"] = userName
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła kod resetowania hasła
    /// </summary>
    public static async Task<string> SendPasswordResetCodeAsync(
        this ISmsService smsService,
        string phoneNumber,
        string code,
        int? userId = null)
    {
        var response = await smsService.SendFromTemplateAsync(
            "password_reset",
            phoneNumber,
            new Dictionary<string, string>
            {
                ["code"] = code
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła powiadomienie o nowym zamówieniu
    /// </summary>
    public static async Task<string> SendOrderNotificationAsync(
        this ISmsService smsService,
        string phoneNumber,
        string orderId,
        decimal amount,
        int? userId = null)
    {
        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"Zamówienie #{orderId} zostało złożone. Kwota: {amount:F2} PLN. Dziękujemy!",
                Sender = "Lumo"
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła przypomnienie o nadchodzącym wydarzeniu
    /// </summary>
    public static async Task<string> SendEventReminderAsync(
        this ISmsService smsService,
        string phoneNumber,
        string eventName,
        DateTimeOffset eventDate,
        int? userId = null)
    {
        var timeUntilEvent = eventDate - DateTimeOffset.UtcNow;
        var timeDescription = timeUntilEvent.Days > 0
            ? $"{timeUntilEvent.Days} dni"
            : $"{timeUntilEvent.Hours} godzin";

        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"Przypomnienie: {eventName} za {timeDescription}. Data: {eventDate:dd.MM.yyyy HH:mm}",
                ScheduledAt = eventDate.AddHours(-24) // Wyślij 24h przed
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła powiadomienie o promocji do wielu odbiorców
    /// </summary>
    public static async Task<SendBulkSmsResponse> SendPromotionAsync(
        this ISmsService smsService,
        List<string> phoneNumbers,
        string promoCode,
        int discountPercent,
        int? userId = null)
    {
        var response = await smsService.SendBulkSmsAsync(
            new SendBulkSmsRequest
            {
                To = phoneNumbers,
                Message = $"Promocja! Użyj kodu {promoCode} i otrzymaj {discountPercent}% zniżki!",
                Sender = "Lumo"
            },
            userId
        );

        return response;
    }

    /// <summary>
    /// Wysyła pilne powiadomienie flash
    /// </summary>
    public static async Task<string> SendUrgentAlertAsync(
        this ISmsService smsService,
        string phoneNumber,
        string message,
        int? userId = null)
    {
        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"PILNE: {message}",
                IsFlash = true
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła potwierdzenie płatności
    /// </summary>
    public static async Task<string> SendPaymentConfirmationAsync(
        this ISmsService smsService,
        string phoneNumber,
        string transactionId,
        decimal amount,
        int? userId = null)
    {
        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"Płatność #{transactionId} na kwotę {amount:F2} PLN została zrealizowana. Dziękujemy!",
                Sender = "Lumo"
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła powiadomienie o zmianie statusu
    /// </summary>
    public static async Task<string> SendStatusChangeAsync(
        this ISmsService smsService,
        string phoneNumber,
        string itemName,
        string oldStatus,
        string newStatus,
        int? userId = null)
    {
        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"{itemName}: status zmieniony z '{oldStatus}' na '{newStatus}'",
                Sender = "Lumo"
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Wysyła ankietę NPS (Net Promoter Score)
    /// </summary>
    public static async Task<string> SendNpsSurveyAsync(
        this ISmsService smsService,
        string phoneNumber,
        string surveyLink,
        int? userId = null)
    {
        var response = await smsService.SendSmsAsync(
            new SendSmsRequest
            {
                To = phoneNumber,
                Message = $"Jak oceniasz nasze usługi? Wypełnij krótką ankietę: {surveyLink}",
                Sender = "Lumo"
            },
            userId
        );

        return response.MessageId;
    }

    /// <summary>
    /// Generuje losowy kod weryfikacyjny
    /// </summary>
    public static string GenerateVerificationCode(int length = 6)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
    }

    /// <summary>
    /// Formatuje numer telefonu do formatu polskiego
    /// </summary>
    public static string FormatPolishPhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.Length == 9)
        {
            return $"+48 {digits[..3]} {digits[3..6]} {digits[6..]}";
        }

        if (digits.Length == 11 && digits.StartsWith("48"))
        {
            var number = digits[2..];
            return $"+48 {number[..3]} {number[3..6]} {number[6..]}";
        }

        return phoneNumber;
    }

    /// <summary>
    /// Waliduje polski numer telefonu
    /// </summary>
    public static bool IsValidPolishPhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // 9 cyfr
        if (digits.Length == 9)
            return true;

        // 11 cyfr z prefiksem 48
        if (digits.Length == 11 && digits.StartsWith("48"))
            return true;

        // 12 cyfr z + na początku (ale + jest usuwany w digits)
        if (phoneNumber.StartsWith("+") && digits.Length == 11 && digits.StartsWith("48"))
            return true;

        return false;
    }

    /// <summary>
    /// Oblicza liczbę części SMS-a
    /// </summary>
    public static int CalculateSmsParts(string message)
    {
        // SMS standard: 160 znaków
        // SMS z polskimi znakami (Unicode): 70 znaków
        // SMS wieloczęściowy: 153/67 znaków na część

        bool hasUnicode = message.Any(c => c > 127);

        if (message.Length == 0)
            return 0;

        if (hasUnicode)
        {
            // Unicode
            if (message.Length <= 70)
                return 1;
            return (int)Math.Ceiling((double)message.Length / 67);
        }
        else
        {
            // Standard GSM
            if (message.Length <= 160)
                return 1;
            return (int)Math.Ceiling((double)message.Length / 153);
        }
    }

    /// <summary>
    /// Szacuje koszt SMS-a
    /// </summary>
    public static decimal EstimateSmsCost(string message, decimal costPerPart = 0.08m)
    {
        var parts = CalculateSmsParts(message);
        return parts * costPerPart;
    }

    /// <summary>
    /// Skraca wiadomość do maksymalnej liczby części
    /// </summary>
    public static string TruncateToMaxParts(string message, int maxParts = 3)
    {
        var parts = CalculateSmsParts(message);

        if (parts <= maxParts)
            return message;

        bool hasUnicode = message.Any(c => c > 127);
        var maxLength = hasUnicode 
            ? (maxParts == 1 ? 70 : maxParts * 67) 
            : (maxParts == 1 ? 160 : maxParts * 153);

        if (message.Length <= maxLength)
            return message;

        return message[..(maxLength - 3)] + "...";
    }
}
