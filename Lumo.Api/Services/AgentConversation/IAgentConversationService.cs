using Lumo.Api.ApiModels.Chat;
using static Lumo.Api.Services.AgentConversation.AgentConversationService;

namespace Lumo.Api.Services.AgentConversation
{
    public interface IAgentConversationService
    {
        IAsyncEnumerable<string> StreamMessageAsync(string userId, ChatMessageRequest request, CancellationToken cancellationToken = default);
        Task<AgentMessageResponse> SendMessageAsync(string userId, ChatMessageRequest request, CancellationToken cancellationToken = default);
        Task<ApprovalResult?> RespondToApprovalAsync(string userId, bool approved, CancellationToken cancellationToken = default);
    }
}
