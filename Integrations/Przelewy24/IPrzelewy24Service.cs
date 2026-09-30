using Integrations.Przelewy24.Models;
using Lumo.Infrastructure.Entities;

namespace Integrations.Przelewy24;

/// <summary>
/// Serwis do obsługi płatności Przelewy24
/// </summary>
public interface IPrzelewy24Service
{
    /// <summary>
    /// Tworzy nową płatność dla użytkownika
    /// </summary>
    Task<CreatePaymentResponse> CreatePaymentAsync(
        string userId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Weryfikuje płatność po powrocie użytkownika
    /// </summary>
    Task<bool> VerifyPaymentAsync(
        string sessionId,
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obsługuje powiadomienie webhook z Przelewy24
    /// </summary>
    Task<bool> HandleNotificationAsync(
        P24NotificationRequest notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pobiera status płatności
    /// </summary>
    Task<Payment?> GetPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pobiera płatność po Session ID
    /// </summary>
    Task<Payment?> GetPaymentBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
}
