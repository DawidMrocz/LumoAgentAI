using Lumo.Api.ApiModels.Chat;
using Lumo.Api.Services.AgentConversation;
using Lumo.Api.Services.AgentUserContext;
using Lumo.Api.Services.Voice;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text;

namespace Lumo.Api.Hubs;

[Authorize]
public sealed class VoiceHub : Hub
{
    private readonly IAgentConversationService _agentConversationService;
    private readonly IVoiceService _voiceService;

    public VoiceHub(
        IAgentConversationService agentConversationService,
        IVoiceService voiceService)
    {
        _agentConversationService = agentConversationService;
        _voiceService = voiceService;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine(
            $"VoiceHub connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        Console.WriteLine(
            $"VoiceHub disconnected: {Context.ConnectionId}");

        if (exception != null)
        {
            Console.WriteLine(
                $"VoiceHub error: {exception.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendVoice(
        string audioBase64)
    {

        var cancellationToken = Context.ConnectionAborted;

        try
        {
            // =====================================================
            // VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(audioBase64))
            {
                await SendErrorAsync(
                    "Nie przesłano nagrania.",
                    cancellationToken);

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // =====================================================
            // USER
            // =====================================================

            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException(
                    "Nie można ustalić użytkownika.");
            }

            // =====================================================
            // BASE64 -> AUDIO
            // =====================================================

            byte[] audio;

            try
            {
                audio = Convert.FromBase64String(audioBase64);
            }
            catch (FormatException)
            {
                await SendErrorAsync(
                    "Nieprawidłowy format nagrania.",
                    cancellationToken);

                return;
            }

            if (audio.Length == 0)
            {
                await SendErrorAsync(
                    "Nagranie jest puste.",
                    cancellationToken);

                return;
            }

            Console.WriteLine(
                $"Voice request: user={userId}, bytes={audio.Length}");

            cancellationToken.ThrowIfCancellationRequested();

            // =====================================================
            // THINKING
            // =====================================================

            await Clients.Caller.SendAsync(
                "AgentThinking",
                cancellationToken);

            // =====================================================
            // STT
            // =====================================================

            Console.WriteLine("STT start");

            var text =
                await _voiceService.SpeechToTextAsync(
                    audio);

            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine(
                $"STT result: {text}");

            if (string.IsNullOrWhiteSpace(text))
            {
                await SendErrorAsync(
                    "Nie udało się rozpoznać mowy.",
                    cancellationToken);

                return;
            }

            // =====================================================
            // SEND TRANSCRIPTION
            // =====================================================

            await Clients.Caller.SendAsync(
                "UserTranscription",
                text,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // =====================================================
            // CHAT REQUEST
            // =====================================================

            var request = new ChatMessageRequest
            {
                Text = text,
                Attachment = null
            };

            // =====================================================
            // AGENT
            // =====================================================

            Console.WriteLine("Agent start");

            var responseBuilder =
                new StringBuilder();

            await foreach (
                var chunk in _agentConversationService
                    .StreamMessageAsync(
                        userId,
                        request,
                        cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(chunk))
                    continue;

                responseBuilder.Append(chunk);

                await Clients.Caller.SendAsync(
                    "AgentChunk",
                    chunk,
                    cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var responseText =
                responseBuilder.ToString();

            Console.WriteLine(
                $"Agent response: {responseText}");

            // =====================================================
            // AGENT COMPLETED
            // =====================================================

            await Clients.Caller.SendAsync(
                "AgentCompleted",
                cancellationToken);

            if (string.IsNullOrWhiteSpace(responseText))
                return;

            // =====================================================
            // TTS
            // =====================================================

            Console.WriteLine("TTS start");

            cancellationToken.ThrowIfCancellationRequested();

            var responseAudio =
                await _voiceService.TextToSpeechAsync(
                    responseText);

            cancellationToken.ThrowIfCancellationRequested();

            if (responseAudio is null ||
                responseAudio.Length == 0)
            {
                return;
            }

            // =====================================================
            // SEND AUDIO
            // =====================================================

            var responseAudioBase64 =
                Convert.ToBase64String(responseAudio);

            await Clients.Caller.SendAsync(
                "AgentAudio",
                responseAudioBase64,
                cancellationToken);

            Console.WriteLine(
                $"TTS sent: {responseAudio.Length} bytes");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Clients.Caller.SendAsync(
                    "AgentCancelled");
            }
            catch
            {
                // Klient może być już rozłączony.
            }
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"VoiceHub ERROR: {ex}");

            try
            {
                await Clients.Caller.SendAsync(
                    "AgentError",
                    "Nie udało się przetworzyć wiadomości głosowej.");
            }
            catch
            {
                // Klient może być już rozłączony.
            }
        }
    }

    private string GetUserId()
    {
        return Context.User?
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException(
                "Nie można ustalić użytkownika.");
    }

    private Task SendErrorAsync(
        string message,
        CancellationToken cancellationToken)
    {
        return Clients.Caller.SendAsync(
            "AgentError",
            message,
            cancellationToken);
    }
}