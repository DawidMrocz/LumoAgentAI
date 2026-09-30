namespace Lumo.Api.Services.ChatRateLimiter
{
    public interface IChatRateLimiterService
    {
        bool TryAcquire(string userId);
    }
}
