using Lumo.Api.DTO.AgentTask;

namespace Lumo.Api.Services.AgentTasks
{
    public interface IAgentTaskService
    {
        Task CreateAgentTaskAsync(CreateAgentTaskToolRequest request, string userOid);
        Task DeleteAgentTaskAsync(int id, string userOid);
        Task<List<AgentTaskResponse>> ListAgentTasksAsync(string userOid);
    }
}
