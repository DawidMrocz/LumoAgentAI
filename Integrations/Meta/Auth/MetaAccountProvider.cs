using Integrations.Meta.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;

namespace Integrations.Meta.Auth
{
    internal sealed class MetaAccountProvider : IMetaAccountProvider
    {
        private const string ProtectorPurpose =
            "Lumo.Meta.OAuth.AccessToken";

        private readonly IMetaAccountRepository _metaAccountRepository;
        private readonly IDataProtector _tokenProtector;

        public MetaAccountProvider(
            IMetaAccountRepository metaAccountRepository,
            IDataProtectionProvider dataProtectionProvider)
        {
            _metaAccountRepository = metaAccountRepository;

            _tokenProtector =
                dataProtectionProvider.CreateProtector(ProtectorPurpose);
        }

        public async Task<MetaAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var entity =
                await _metaAccountRepository.GetAsync(
                    userId,
                    cancellationToken);

            if (entity is null)
                return null;

            return new MetaAccount
            {
                UserId = entity.UserId,
                MetaUserId = entity.MetaUserId,
                Email = entity.Email,

                AccessTokenProtected =
                    _tokenProtector.Unprotect(
                        entity.AccessTokenProtected),

                GrantedScopes = entity.GrantedScopes,
                TokenExpiresAtUtc = entity.TokenExpiresAtUtc
            };
        }

        public async Task SaveAsync(
     SaveMetaAccountRequest request,
     CancellationToken cancellationToken = default)
        {
            var existing =
                await _metaAccountRepository.GetAsync(
                    request.UserId,
                    cancellationToken);

            string protectedAccessToken;

            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                protectedAccessToken =
                    _tokenProtector.Protect(request.AccessToken);
            }
            else if (existing is not null)
            {
                protectedAccessToken =
                    existing.AccessTokenProtected;
            }
            else
            {
                throw new InvalidOperationException(
                    "Meta nie zwróciła access tokena dla nowego konta.");
            }

            var now = DateTime.UtcNow;

            var account = new MetaAccount
            {
                Id = existing?.Id ?? 0,

                UserId = request.UserId,

                MetaUserId = request.MetaUserId,

                Email = request.Email,

                AccessTokenProtected = protectedAccessToken,

                GrantedScopes = request.GrantedScopes ?? string.Empty,

                TokenExpiresAtUtc = request.TokenExpiresAtUtc,

                CreatedAt = existing?.CreatedAt ?? now,

                UpdatedAt = now
            };

            await _metaAccountRepository.SaveAsync(
                account,
                cancellationToken);
        }

        public Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _metaAccountRepository.DeleteAsync(
                userId,
                cancellationToken);
        }
    }
}