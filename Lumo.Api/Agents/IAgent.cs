namespace Lumo.Api.Agents
{
    public interface IAgent
    {
        string Name { get;  }
        string Description { get; }
        Task<string> ExecuteAsync(string instruction, CancellationToken cancellationToken = default);
    }
}
