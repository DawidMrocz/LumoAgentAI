using Lumo.Api.ApiModels.Chat;
using Lumo.Api.Services.AgentConversation;
using Lumo.Api.Services.ChatRateLimiter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Lumo.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private const int MaxMessageLength = 10_000;

    private readonly IAgentConversationService _agentConversationService;
    private readonly IChatRateLimiterService _chatRateLimiterService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IAgentConversationService agentConversationService,
        IChatRateLimiterService chatRateLimiterService,
        ILogger<ChatHub> logger)
    {
        _agentConversationService = agentConversationService;
        _chatRateLimiterService = chatRateLimiterService;
        _logger = logger;
    }

    /// <summary>
    /// Wysyła odpowiedź do klienta fragmentami.
    /// </summary>
    public async Task SendMessage(ChatMessageRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;

        ValidateRequest(request);

        var userId = GetUserId();

        EnsureRateLimit(userId);

        try
        {
            await foreach (var chunk in
                _agentConversationService.StreamMessageAsync(
                    userId,
                    request,
                    cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Clients.Caller.SendAsync(
                    "ReceiveChunk",
                    chunk,
                    cancellationToken);
            }

            await Clients.Caller.SendAsync(
                "MessageCompleted",
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Chat stream cancelled. UserId: {UserId}",
                userId);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing streaming chat message. UserId: {UserId}",
                userId);

            throw new HubException(
                "Wystąpił błąd podczas przetwarzania wiadomości.");
        }
    }

    /// <summary>
    /// Wysyła odpowiedź do klienta dopiero po jej całkowitym wygenerowaniu.
    /// Obsługuje również żądania akceptacji wykonania toola.
    /// </summary>
    public async Task SendMessageComplete(ChatMessageRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;

        ValidateRequest(request);

        var userId = GetUserId();

        EnsureRateLimit(userId);

        try
        {
            var response =
                await _agentConversationService.SendMessageAsync(
                    userId,
                    request,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await Clients.Caller.SendAsync(
                "MessageCompleted",
                response,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Chat request cancelled. UserId: {UserId}",
                userId);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing complete chat message. UserId: {UserId}",
                userId);

            throw new HubException(
                "Wystąpił błąd podczas przetwarzania wiadomości.");
        }
    }

    /// <summary>
    /// Odpowiada na żądanie akceptacji wykonania narzędzia.
    /// </summary>
    public async Task RespondToApproval(
        bool approved)
    {
        var cancellationToken = Context.ConnectionAborted;

        var userId = GetUserId();

        try
        {
            var result =
                await _agentConversationService.RespondToApprovalAsync(
                    userId,
                    approved,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (result is null)
            {
                await Clients.Caller.SendAsync(
                    "ApprovalCompleted",
                    new
                    {
                        Success = false,
                        Message = "Brak oczekującego żądania akceptacji."
                    },
                    cancellationToken);

                return;
            }

            await Clients.Caller.SendAsync(
                "ApprovalCompleted",
                result,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Approval request cancelled. UserId: {UserId}",
                userId);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing tool approval. UserId: {UserId}",
                userId);

            throw new HubException(
                "Wystąpił błąd podczas przetwarzania akceptacji.");
        }
    }

    private static void ValidateRequest(
        ChatMessageRequest request)
    {
        if (request is null)
            throw new HubException(
                "Nieprawidłowa wiadomość.");

        // Wiadomość może zawierać:
        // - tekst,
        // - załącznik,
        // - albo oba.
        if (string.IsNullOrWhiteSpace(request.Text) &&
            request.Attachment is null)
        {
            throw new HubException(
                "Wiadomość jest pusta.");
        }

        if (request.Text?.Length > MaxMessageLength)
        {
            throw new HubException(
                $"Wiadomość może mieć maksymalnie " +
                $"{MaxMessageLength:N0} znaków.");
        }
    }

    private string GetUserId()
    {
        return Context.User?
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException(
                "Nie można ustalić użytkownika.");
    }

    private void EnsureRateLimit(
        string userId)
    {
        if (!_chatRateLimiterService.TryAcquire(userId))
        {
            throw new HubException(
                "Osiągnięto limit wiadomości. " +
                "Spróbuj ponownie za chwilę.");
        }
    }
}