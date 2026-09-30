using Microsoft.Extensions.AI;

namespace Lumo.Api.Services.AgentTools
{
    public interface IAgentToolsService
    {
        IEnumerable<AIFunction> GetTools();
    }
}
