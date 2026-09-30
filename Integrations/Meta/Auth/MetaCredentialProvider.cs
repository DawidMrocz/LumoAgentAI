using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Meta.Auth
{
    internal sealed class MetaCredentialProvider
    : IMetaCredentialProvider
    {
        private readonly IMetaAccountProvider _metaAccountProvider;

        public MetaCredentialProvider(
            IMetaAccountProvider metaAccountProvider)
        {
            _metaAccountProvider = metaAccountProvider;
        }

        public async Task<string> GetAccessTokenAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var account =
                await _metaAccountProvider.GetAsync(
                    userId,
                    cancellationToken);

            if (account is null)
            {
                throw new InvalidOperationException(
                    "Meta: Użytkownik nie ma połączonego konta.");
            }

            if (string.IsNullOrWhiteSpace(
                    account.AccessTokenProtected))
            {
                throw new InvalidOperationException(
                    "Meta: Brak access tokena.");
            }

            if (account.TokenExpiresAtUtc.HasValue &&
                account.TokenExpiresAtUtc.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Meta: Access token wygasł.");
            }

            return account.AccessTokenProtected;
        }
    }
}
