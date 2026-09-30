using Microsoft.Agents.AI;

namespace Lumo.Api.Services.Agent
{
    public interface IAgentService
    {
        AIAgent GetAgent();
    }
}
