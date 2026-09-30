namespace Lumo.Api.Services.AgentUserContext
{
    public interface IAgentUserContextService
    {
        string UserId { get; }
        IDisposable Push(string userId);
    }
}
