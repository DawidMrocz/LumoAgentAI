using Lumo.Infrastructure.Repositories;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Integrations.Twilio.Client
{
    internal sealed class TwilioClient : ITwilioClient
    {
        private readonly ISmsConfigRepository _configRepository;

        public TwilioClient(ISmsConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public async Task<MessageResource> SendAsync(
            string to,
            string from,
            string message,
            DateTimeOffset? scheduledAt = null,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateRestClientAsync(cancellationToken);

            var options = new CreateMessageOptions(new PhoneNumber(to))
            {
                From = new PhoneNumber(from),
                Body = message
            };

            if (scheduledAt.HasValue && scheduledAt.Value > DateTimeOffset.UtcNow)
            {
                options.SendAt = scheduledAt.Value.UtcDateTime;
            }

            return await MessageResource.CreateAsync(options, client);
        }

        public async Task<MessageResource> GetMessageAsync(
            string messageSid,
            CancellationToken cancellationToken = default)
        {
            var client = await CreateRestClientAsync(cancellationToken);

            return await MessageResource.FetchAsync(pathSid: messageSid, client: client);
        }

        private async Task<ITwilioRestClient> CreateRestClientAsync(CancellationToken cancellationToken)
        {
            var config = await _configRepository.GetAsync(cancellationToken)
                ?? throw new InvalidOperationException("Twilio: Brak konfiguracji SMS.");

            if (string.IsNullOrWhiteSpace(config.AccountSid) || string.IsNullOrWhiteSpace(config.AuthToken))
            {
                throw new InvalidOperationException("Twilio: Brak AccountSid/AuthToken w konfiguracji.");
            }

            return new TwilioRestClient(config.AccountSid, config.AuthToken);
        }
    }
}
