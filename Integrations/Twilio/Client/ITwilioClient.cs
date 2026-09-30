using Twilio.Rest.Api.V2010.Account;

namespace Integrations.Twilio.Client
{
    public interface ITwilioClient
    {
        Task<MessageResource> SendAsync(
            string to,
            string from,
            string message,
            DateTimeOffset? scheduledAt = null,
            CancellationToken cancellationToken = default);

        Task<MessageResource> GetMessageAsync(
            string messageSid,
            CancellationToken cancellationToken = default);
    }
}
