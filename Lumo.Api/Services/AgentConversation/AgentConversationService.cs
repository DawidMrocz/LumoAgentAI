using Lumo.Api.ApiModels.Chat;
using Lumo.Api.Services.Agent;
using Lumo.Api.Services.AgentSessionStore;
using Lumo.Api.Services.AgentUserContext;
using Lumo.Api.Services.Moderation;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Lumo.Api.Services.AgentConversation;

public sealed class AgentConversationService : IAgentConversationService
{
    private const int StreamBufferSize = 256;

    private static readonly TimeSpan StreamFlushInterval =
        TimeSpan.FromMilliseconds(40);

    private readonly IAgentService _agentService;
    private readonly IAgentSessionStoreService _agentSessionStoreService;
    private readonly IOpenAIModerationService _moderationService;
    private readonly IAgentUserContextService _agentUserContextService;

    public AgentConversationService(IAgentService agentService, IAgentSessionStoreService agentSessionStoreService, IOpenAIModerationService moderationService, IAgentUserContextService agentUserContextService)
    {
        _agentService = agentService;
        _agentSessionStoreService = agentSessionStoreService;
        _moderationService = moderationService;
        _agentUserContextService = agentUserContextService;
    }

    // ============================================================
    // STREAMING
    // ============================================================

    public async IAsyncEnumerable<string> StreamMessageAsync(
        string userId,
        ChatMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        var text = request.Text?.Trim();
        var attachment = request.Attachment;

        if (string.IsNullOrWhiteSpace(text) && attachment is null)
        {
            yield return "Nie podano wiadomości.";
            yield break;
        }

        // Moderujemy tylko tekst.
        // Attachment jest przekazywany do odpowiedniego toola.
        if (!string.IsNullOrWhiteSpace(text))
        {
            var moderation =
                await _moderationService.ModerateAsync(
                    text,
                    cancellationToken);

            if (!moderation.IsAllowed)
            {
                yield return "Nie mogę pomóc z tą wiadomością.";
                yield break;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        var agent = _agentService.GetAgent();
        var conversationId = userId;

        var input =
            BuildAgentInput(
                text,
                attachment);

        using var userScope =
            _agentUserContextService.Push(userId);

        await using var handle =
            await _agentSessionStoreService.GetSessionAsync(
                conversationId,
                agent,
                cancellationToken);

        var session = handle.Session;

        var buffer = new StringBuilder();
        var lastFlush = Stopwatch.GetTimestamp();

        await foreach (
            var update in agent.RunStreamingAsync(
                input,
                session,
                cancellationToken: cancellationToken))
        {
            if (string.IsNullOrEmpty(update.Text))
                continue;

            buffer.Append(update.Text);

            var elapsed =
                Stopwatch.GetElapsedTime(lastFlush);

            if (elapsed >= StreamFlushInterval ||
                buffer.Length >= StreamBufferSize)
            {
                yield return buffer.ToString();

                buffer.Clear();

                lastFlush =
                    Stopwatch.GetTimestamp();
            }
        }

        if (buffer.Length > 0)
            yield return buffer.ToString();
    }

    // ============================================================
    // NORMAL MESSAGE
    // ============================================================

    public async Task<AgentMessageResponse> SendMessageAsync(
        string userId,
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        var text = request.Text?.Trim();
        var attachment = request.Attachment;

        if (string.IsNullOrWhiteSpace(text) && attachment is null)
        {
            return new AgentMessageResponse
            {
                Text = "Nie podano wiadomości."
            };
        }

        // Moderacja tekstu.
        if (!string.IsNullOrWhiteSpace(text))
        {
            var moderation =
                await _moderationService.ModerateAsync(
                    text,
                    cancellationToken);

            if (!moderation.IsAllowed)
            {
                return new AgentMessageResponse
                {
                    Text = "Nie mogę pomóc z tą wiadomością."
                };
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        var agent = _agentService.GetAgent();
        var conversationId = userId;

        var input =
            BuildAgentInput(
                text,
                attachment);

        using var userScope =
            _agentUserContextService.Push(userId);

        await using var handle =
            await _agentSessionStoreService.GetSessionAsync(
                conversationId,
                agent,
                cancellationToken);

        var response =
            await agent.RunAsync(
                input,
                handle.Session,
                cancellationToken: cancellationToken);

        var approvalRequest =
            response.Messages
                .SelectMany(x => x.Contents)
                .OfType<ToolApprovalRequestContent>()
                .FirstOrDefault();

        if (approvalRequest is not null)
        {
            handle.PendingApproval =
                approvalRequest;

            var functionCall =
                approvalRequest.ToolCall
                    as FunctionCallContent;

            var arguments =
                functionCall?.Arguments is not null
                    ? JsonSerializer.Serialize(
                        functionCall.Arguments)
                    : null;

            return new AgentMessageResponse
            {
                Approval =
                    new ToolApprovalRequestDto(
                        functionCall?.Name ?? "unknown",
                        arguments)
            };
        }

        return new AgentMessageResponse
        {
            Text = response.Text ?? string.Empty
        };
    }

    // ============================================================
    // APPROVAL
    // ============================================================

    public async Task<ApprovalResult?> RespondToApprovalAsync(
        string userId,
        bool approved,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        cancellationToken.ThrowIfCancellationRequested();

        var agent =
            _agentService.GetAgent();

        using var userScope =
            _agentUserContextService.Push(userId);

        await using var handle =
            await _agentSessionStoreService.GetSessionAsync(
                userId,
                agent,
                cancellationToken);

        var approvalRequest =
            handle.PendingApproval;

        if (approvalRequest is null)
            return null;

        // Approval może zostać wykorzystany tylko raz.
        handle.PendingApproval = null;

        var approvalMessage =
            new ChatMessage(
                ChatRole.User,
                [
                    approvalRequest.CreateResponse(approved)
                ]);

        var jsonSerializerOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver =
                    new DefaultJsonTypeInfoResolver(),
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

        var options =
            new AgentRunOptions
            {
                ResponseFormat =
                    ChatResponseFormat
                        .ForJsonSchema<ApprovalResponse>(
                            jsonSerializerOptions)
            };

        AgentResponse response =
            await agent.RunAsync<ApprovalResponse>(
                approvalMessage,
                handle.Session,
                options: options,
                cancellationToken: cancellationToken);

        // Agent może poprosić o kolejny approval.
        var nextApprovalRequest =
            response.Messages
                .SelectMany(x => x.Contents)
                .OfType<ToolApprovalRequestContent>()
                .FirstOrDefault();

        if (nextApprovalRequest is not null)
        {
            handle.PendingApproval =
                nextApprovalRequest;

            var functionCall =
                nextApprovalRequest.ToolCall
                    as FunctionCallContent;

            var arguments =
                functionCall?.Arguments is not null
                    ? JsonSerializer.Serialize(
                        functionCall.Arguments,
                        jsonSerializerOptions)
                    : null;

            return new ApprovalResult
            {
                Approval =
                    new ToolApprovalRequestDto(
                        functionCall?.Name ?? "unknown",
                        arguments)
            };
        }

        var result =
            JsonSerializer.Deserialize<ApprovalResponse>(
                response.Text,
                jsonSerializerOptions)
            ?? throw new InvalidOperationException(
                "Nie udało się zdeserializować odpowiedzi agenta.");

        return new ApprovalResult
        {
            Response = result
        };
    }

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    private static string BuildAgentInput(
        string? text,
        ChatAttachmentRequest? attachment)
    {
        if (attachment is null)
            return text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
            return BuildAttachmentContext(attachment);

        return string.Concat(
            text,
            Environment.NewLine,
            Environment.NewLine,
            BuildAttachmentContext(attachment));
    }

    /// <summary>
    /// Metadane załącznika pochodzą od użytkownika i trafiają do promptu, więc są
    /// niezaufane. Usuwamy znaki nowej linii i przycinamy długość, żeby nazwa pliku
    /// nie mogła udawać kolejnej instrukcji systemowej (prompt injection).
    /// </summary>
    private static string SanitizeForPrompt(string? value, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "(brak)";

        var cleaned = new string(value
            .Where(c => !char.IsControl(c))
            .ToArray())
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        if (cleaned.Length > maxLength)
            cleaned = cleaned[..maxLength];

        return cleaned.Length == 0 ? "(brak)" : cleaned;
    }

    private static string BuildAttachmentContext(
        ChatAttachmentRequest attachment)
    {
        var id = SanitizeForPrompt(attachment.Id);
        var fileName = SanitizeForPrompt(attachment.FileName);
        var contentType = SanitizeForPrompt(attachment.ContentType, 150);

        if (attachment.ContentType.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Użytkownik dołączył obraz.

                Plik:
                - ID: {id}
                - Nazwa: {fileName}
                - Content-Type: {contentType}

                Jeśli pytanie użytkownika dotyczy zawartości obrazu,
                użyj narzędzia AnalyzeImage.
                """;
        }

        if (string.Equals(
                attachment.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Użytkownik dołączył dokument PDF.

                Plik:
                - ID: {id}
                - Nazwa: {fileName}
                - Content-Type: {contentType}

                Jeśli pytanie użytkownika dotyczy informacji
                znajdujących się w dokumencie, użyj narzędzia
                SearchUploadedFile.
                """;
        }

        return $"""
            Użytkownik dołączył plik.

            Plik:
            - ID: {id}
            - Nazwa: {fileName}
            - Content-Type: {contentType}
            - Rozmiar: {attachment.Size} bajtów
            """;
    }

    // ============================================================
    // APPROVAL DTOs
    // ============================================================

    public sealed class ApprovalResponse
    {
        public string Decision { get; set; } = "";
        public string Reason { get; set; } = "";
    }

    public sealed class ApprovalResult
    {
        public ApprovalResponse? Response { get; init; }

        public ToolApprovalRequestDto? Approval { get; init; }
    }

    public sealed record ToolApprovalRequestDto(
        string ToolName,
        string? Arguments);

    public sealed class AgentMessageResponse
    {
        public string? Text { get; init; }

        public ToolApprovalRequestDto? Approval { get; init; }
    }
}