namespace Lumo.Api.Services.AgentUserContext
{
    public sealed class AgentUserContextService : IAgentUserContextService
    {
        private static readonly AsyncLocal<string?> CurrentUserId = new();
        public string UserId => CurrentUserId.Value ?? throw new InvalidOperationException("Agent user context is not initialized.");

        public IDisposable Push(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            var previous = CurrentUserId.Value;
            CurrentUserId.Value = userId.Trim();
            return new Scope(previous);
        }

        private sealed class Scope : IDisposable
        {
            private readonly string? _previous;
            private bool _disposed;

            public Scope(string? previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                CurrentUserId.Value = _previous;
            }
        }
    }
}
