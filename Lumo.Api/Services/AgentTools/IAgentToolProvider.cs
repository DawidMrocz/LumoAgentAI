using Microsoft.Extensions.AI;

namespace Lumo.Api.Services.AgentTools;

public interface IAgentToolProvider
{
    IEnumerable<AITool> GetTools();
}