using Microsoft.Agents.AI;

namespace Lumo.Api.Services.AgentSessionStore
{
    public interface IAgentSessionStoreService
    {
        Task<AgentSessionHandle> GetSessionAsync(string conversationId, AIAgent agent, CancellationToken cancellationToken = default);

    }
}
