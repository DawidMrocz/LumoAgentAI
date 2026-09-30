using System.Collections.Concurrent;

namespace Lumo.Api.Services.ChatRateLimiter
{
    public sealed class ChatRateLimiterService : IChatRateLimiterService, IDisposable
    {
        private const int MaxRequests = 10;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<string, Entry> _users = new();
        private readonly Timer _sweeper;

        public ChatRateLimiterService()
        {
            // Bez tego słownik rósł w nieskończoność - jeden wpis na każdego użytkownika,
            // który kiedykolwiek wysłał wiadomość, trzymany do restartu procesu.
            _sweeper = new Timer(_ => Sweep(), null, SweepInterval, SweepInterval);
        }

        public bool TryAcquire(string userId)
        {
            var now = DateTimeOffset.UtcNow;

            var entry = _users.GetOrAdd(userId, _ => new Entry(now));

            lock (entry)
            {
                if (now - entry.WindowStart >= Window)
                {
                    entry.WindowStart = now;
                    entry.Count = 0;
                }

                if (entry.Count >= MaxRequests)
                    return false;

                entry.Count++;
                return true;
            }
        }

        private void Sweep()
        {
            var cutoff = DateTimeOffset.UtcNow - Window;

            foreach (var (userId, entry) in _users)
            {
                bool stale;

                lock (entry)
                {
                    stale = entry.WindowStart < cutoff;
                }

                if (!stale)
                    continue;

                // Usuwamy tylko wtedy, gdy wpis nadal jest ten sam i nadal przeterminowany -
                // inaczej moglibyśmy skasować okno założone równolegle przez inne żądanie.
                if (_users.TryGetValue(userId, out var current) && ReferenceEquals(current, entry))
                {
                    lock (entry)
                    {
                        if (entry.WindowStart < cutoff)
                            _users.TryRemove(new KeyValuePair<string, Entry>(userId, entry));
                    }
                }
            }
        }

        public void Dispose()
        {
            _sweeper.Dispose();
        }

        private sealed class Entry(DateTimeOffset windowStart)
        {
            public DateTimeOffset WindowStart { get; set; } = windowStart;
            public int Count { get; set; }
        }
    }
}
