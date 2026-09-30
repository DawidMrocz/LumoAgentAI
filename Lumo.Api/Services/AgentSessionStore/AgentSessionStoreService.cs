using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;

namespace Lumo.Api.Services.AgentSessionStore
{
    public sealed class AgentSessionStoreService : IAgentSessionStoreService
    {
        private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Maksymalny czas oczekiwania na zwolnienie sesji przez inne żądanie tego
        /// samego użytkownika. Bez limitu zawieszony stream blokowałby użytkownika
        /// bezterminowo.
        /// </summary>
        private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(90);

        /// <summary>
        /// IMemoryCache jest współdzielony z resztą aplikacji, więc klucz musi być
        /// prefiksowany - samo conversationId (równe userId) mogło kolidować
        /// z wpisami innych serwisów.
        /// </summary>
        private const string CacheKeyPrefix = "agent-session:";

        private readonly IMemoryCache _cache;

        public AgentSessionStoreService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<AgentSessionHandle> GetSessionAsync(string conversationId, AIAgent agent, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
            ArgumentNullException.ThrowIfNull(agent);

            var entry = _cache.GetOrCreate(
                CacheKeyPrefix + conversationId,
                cacheEntry =>
                {
                    cacheEntry.SetSlidingExpiration(SessionTtl);
                    return new AgentSessionEntry();
                })!;

            var acquired = await entry.Gate.WaitAsync(GateTimeout, cancellationToken);

            if (!acquired)
            {
                throw new InvalidOperationException(
                    "Poprzednie żądanie tej rozmowy wciąż jest przetwarzane. Spróbuj ponownie za chwilę.");
            }

            try
            {
                entry.Session ??= await agent.CreateSessionAsync(cancellationToken);

                return new AgentSessionHandle(
                    entry.Session,
                    entry);
            }
            catch
            {
                entry.Gate.Release();
                throw;
            }
        }
    }

    internal sealed class AgentSessionEntry
    {
        public AgentSession? Session { get; set; }

        public ToolApprovalRequestContent? PendingApproval { get; set; }

        public SemaphoreSlim Gate { get; } = new(1, 1);
    }

    public sealed class AgentSessionHandle : IAsyncDisposable
    {
        private readonly SemaphoreSlim _gate;
        private readonly AgentSessionEntry _entry;
        private int _disposed;

        public AgentSession Session { get; }

        internal AgentSessionHandle(
            AgentSession session,
            AgentSessionEntry entry)
        {
            Session = session;
            _entry = entry;
            _gate = entry.Gate;
        }

        public ToolApprovalRequestContent? PendingApproval
        {
            get => _entry.PendingApproval;
            set => _entry.PendingApproval = value;
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return ValueTask.CompletedTask;

            _gate.Release();

            return ValueTask.CompletedTask;
        }
    }
}
